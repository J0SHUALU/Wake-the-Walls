#import <ReplayKit/ReplayKit.h>
#import <Photos/Photos.h>

enum
{
    WtwIdle = 0,
    WtwStarting = 1,
    WtwRecording = 2,
    WtwSaving = 3,
    WtwSaved = 4,
    WtwPhotosDenied = 5,
    WtwFailed = 6
};

static volatile int wtwState = WtwIdle;

static void WtwSaveToPhotos(NSURL *url)
{
    [PHPhotoLibrary requestAuthorizationForAccessLevel:PHAccessLevelAddOnly handler:^(PHAuthorizationStatus status) {
        if (status != PHAuthorizationStatusAuthorized && status != PHAuthorizationStatusLimited)
        {
            [[NSFileManager defaultManager] removeItemAtURL:url error:nil];
            wtwState = WtwPhotosDenied;
            return;
        }
        [[PHPhotoLibrary sharedPhotoLibrary] performChanges:^{
            [PHAssetChangeRequest creationRequestForAssetFromVideoAtFileURL:url];
        } completionHandler:^(BOOL success, NSError *error) {
            [[NSFileManager defaultManager] removeItemAtURL:url error:nil];
            wtwState = success ? WtwSaved : WtwFailed;
        }];
    }];
}

extern "C"
{
    int WtwRecorderIsAvailable()
    {
        return [RPScreenRecorder sharedRecorder].isAvailable ? 1 : 0;
    }

    int WtwRecorderIsRecording()
    {
        return [RPScreenRecorder sharedRecorder].isRecording ? 1 : 0;
    }

    int WtwRecorderState()
    {
        return wtwState;
    }

    void WtwRecorderReset()
    {
        wtwState = WtwIdle;
    }

    void WtwRecorderStart()
    {
        RPScreenRecorder *recorder = [RPScreenRecorder sharedRecorder];
        if (recorder.isRecording) return;
        wtwState = WtwStarting;
        recorder.microphoneEnabled = NO;
        recorder.cameraEnabled = NO;
        [recorder startRecordingWithHandler:^(NSError *error) {
            wtwState = error != nil ? WtwFailed : WtwRecording;
        }];
    }

    void WtwRecorderStop()
    {
        wtwState = WtwSaving;
        NSString *name = [NSString stringWithFormat:@"WakeTheWalls-%@.mp4", [[NSUUID UUID] UUIDString]];
        NSURL *url = [NSURL fileURLWithPath:[NSTemporaryDirectory() stringByAppendingPathComponent:name]];
        [[RPScreenRecorder sharedRecorder] stopRecordingWithOutputURL:url completionHandler:^(NSError *error) {
            if (error != nil)
            {
                [[NSFileManager defaultManager] removeItemAtURL:url error:nil];
                wtwState = WtwFailed;
                return;
            }
            WtwSaveToPhotos(url);
        }];
    }
}
