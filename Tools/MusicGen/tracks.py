from synth import *
import subprocess, sys
def save(name,y):
    import wave
    pcm=(np.clip(y.T,-1,1)*32767).astype('<i2')
    with wave.open(name+'.wav','wb') as w:
        w.setnchannels(2); w.setsampwidth(2); w.setframerate(SR); w.writeframes(pcm.tobytes())
    subprocess.run(['ffmpeg','-y','-loglevel','error','-i',name+'.wav','-c:a','libvorbis','-q:a','5',name+'.ogg'],check=True)

def rooted():  # warm kalimba, earthy, C major pentatonic, 84 bpm
    T=Track(84,16); C=60
    prog=[[48,55,64],[45,52,60],[41,48,57],[43,50,59]]
    pat=[0,2,4,7,9,7,4,2]; scale=[0,2,4,7,9,12,14,16]
    for bar in range(16):
        ch=prog[(bar//2)%4]
        if bar%2==0: T.add(pad([m+12 for m in ch],T.beat*8.5,900),bar*4,0,0.35)
        T.add(kalimba(ch[0],2.0),bar*4,-0.2,0.5)
        for i in range(8):
            if rng.random()<0.85:
                m=C+scale[pat[(i+bar)%8]%8] if bar%4<2 else C+scale[(pat[i]+bar)%8]
                T.add(kalimba(m,1.4),bar*4+i*0.5,(i%3-1)*0.4,0.45)
        for i in range(8): T.add(shaker(amp=0.5 if i%2 else 0.25),bar*4+i*0.5,0.5,0.25)
        T.add(drum(0.5,90,0.18,0.1),bar*4,0,0.35); T.add(drum(0.5,140,0.12,0.2),bar*4+2.5,0.1,0.25)
    return T.render(0.35,2.5)

def bloom():  # dreamy bells over soft pad, D major, 64 bpm
    T=Track(64,12); prog=[[50,57,62,66],[47,54,59,62],[43,50,55,59],[45,52,57,61]]
    for bar in range(12):
        ch=prog[bar%4]
        T.add(pad([m+12 for m in ch],T.beat*5,1500),bar*4-0.5 if bar else 0,0,0.5)
        arp=[ch[1]+12,ch[2]+12,ch[3]+12,ch[2]+24]
        for i,m in enumerate(arp):
            T.add(bell(m,3.0),bar*4+i*1.0+(0.5 if i==3 else 0),(-0.6,0.6,-0.3,0.3)[i],0.22)
        if bar%2==1: T.add(bell(ch[3]+24,4.0),bar*4+3.5,0.7,0.12)
    return T.render(0.6,4.5)

def desert():  # D hijaz, drone, plucked oud-like lines, frame drum, 88 bpm
    T=Track(88,16); D=62; hij=[0,1,4,5,7,8,10,12]
    T.add(drone(38,T.N+SR),0,0,0.08); T.add(drone(50,T.N+SR),0,0.2,0.04)
    phrases=[[0,1,2,1,0],[2,3,4,3,2,1],[4,5,4,3,2,1,0],[7,6,5,4,3,2,1,0]]
    for bar in range(16):
        b=bar*4
        T.add(drum(0.6,75,0.25,0.15),b,0,0.6); T.add(drum(0.3,300,0.06,0.6),b+1.5,0.2,0.35)
        T.add(drum(0.3,300,0.06,0.6),b+2,-0.2,0.3); T.add(drum(0.6,75,0.25,0.15),b+2.5,0,0.45); T.add(drum(0.3,300,0.06,0.6),b+3.5,0.2,0.3)
        if bar%2==0:
            ph=phrases[(bar//2)%4]
            for i,s in enumerate(ph):
                T.add(pluck(D+hij[s]-12,1.2,0.6,0.997),b+i*0.5,-0.3,0.55)
        elif bar%4==3:
            T.add(flute(D+hij[4],T.beat*1.5),b,0.4,0.18); T.add(flute(D+hij[2],T.beat*2),b+1.5,0.4,0.18)
    return T.render(0.4,3.5)

def faces():  # ceremonial drums, mallets, low drone, A minor pentatonic, 96 bpm
    T=Track(96,16); A=57; pent=[0,3,5,7,10,12,15]
    T.add(drone(33,T.N+SR),0,0,0.07)
    bass=[0,0,5,3]; mel=[4,2,3,1,2,0,1,0]
    for bar in range(16):
        b=bar*4
        for s,f,g in [(0,70,0.7),(1.5,70,0.5),(2,180,0.4),(2.75,180,0.3),(3,70,0.5),(3.5,250,0.3)]:
            T.add(drum(0.6,f,0.2 if f<100 else 0.08,0.25),b+s,(-0.3 if f>100 else 0),g)
        T.add(mallet(45+pent[bass[bar%4]],0.8),b,0,0.5)
        for i in range(8):
            if (i+bar)%3!=2: T.add(mallet(A+pent[(mel[i]+bar//4)%7],0.5),b+i*0.5,0.3*((i%2)*2-1),0.3)
        if bar%4==2: T.add(flute(A+12+pent[2],T.beat*3),b+0.5,0.5,0.12)
    return T.render(0.3,2.5)

def orbit():  # bright orbiting arpeggio, light beat, E lydian, 112 bpm
    T=Track(112,16); E=64; prog=[[52,59,64,68,70],[49,56,61,64,68],[45,52,57,61,64],[47,54,59,63,66]]
    for bar in range(16):
        ch=prog[(bar//2)%4]; b=bar*4
        T.add(kick(),b,0,0.6); T.add(kick(),b+2,0,0.55); T.add(kick(),b+2.75,0,0.3)
        for i in range(8): T.add(hat(),b+i*0.5+0.25,0.4*((i%2)*2-1),0.18)
        T.add(drum(0.3,200,0.06,0.9),b+1,0.1,0.25); T.add(drum(0.3,200,0.06,0.9),b+3,-0.1,0.25)
        T.add(pluck(ch[0]-12,T.beat*2,0.3,0.995),b,0,0.6); T.add(pluck(ch[0]-12,T.beat*2,0.3,0.995),b+2.5,0,0.5)
        arp=ch[1:]+[ch[2]+12,ch[3]+12]+ch[1:][::-1][:2]
        for i in range(16):
            m=arp[i%len(arp)]+12
            pan=np.sin(2*np.pi*(b*4+i)/16)*0.8   # the notes circle around the listener
            T.add(bell(m,0.8)*0.6+kalimba(m,0.8)*0.4,b+i*0.25,pan,0.16)
        if bar%2==0: T.add(pad(ch[1:4],T.beat*8.2,2000),b,0,0.25)
    return T.render(0.3,2.0)

for name,fn in [('rooted',rooted),('bloom',bloom),('desert',desert),('faces',faces),('orbit',orbit)]:
    if len(sys.argv)>1 and name not in sys.argv: continue
    y=fn(); save(name,y); print(name, y.shape[1]/SR)
