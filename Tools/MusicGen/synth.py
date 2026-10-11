import numpy as np, sys
from scipy.signal import fftconvolve, butter, sosfilt
SR=44100
rng=np.random.default_rng(7)

def hz(m): return 440*2**((m-69)/12)
def env(n,a,d):
    t=np.arange(n)/SR; e=np.exp(-t/d); k=int(a*SR)
    if k>0: e[:k]*=np.linspace(0,1,k)
    return e
def lp(x,f,o=2): return sosfilt(butter(o,f,'low',fs=SR,output='sos'),x)
def hp(x,f,o=2): return sosfilt(butter(o,f,'high',fs=SR,output='sos'),x)
def bp(x,lo,hi): return sosfilt(butter(2,[lo,hi],'band',fs=SR,output='sos'),x)

def pluck(m,dur,bright=0.5,decay=0.996):
    f=hz(m); p=int(SR/f); n=int(dur*SR)
    buf=lp(rng.uniform(-1,1,p),min(18000,600+bright*8000)) if p>8 else rng.uniform(-1,1,p)
    out=np.zeros(n); b=list(buf)
    b=np.array(b); i=0
    for j in range(n):
        out[j]=b[i]; nxt=(i+1)%p
        b[i]=decay*0.5*(b[i]+b[nxt]); i=nxt
    return out*env(n,0.002,dur)
def kalimba(m,dur):
    n=int(dur*SR); t=np.arange(n)/SR; f=hz(m)
    return (np.sin(2*np.pi*f*t)*env(n,0.003,0.9)+0.35*np.sin(2*np.pi*f*5.4*t)*env(n,0.001,0.08)+0.15*np.sin(2*np.pi*f*2*t)*env(n,0.002,0.4))
def bell(m,dur):
    n=int(dur*SR); t=np.arange(n)/SR; f=hz(m)
    mod=2.0*np.exp(-t/0.6)*np.sin(2*np.pi*f*3.5*t)
    return np.sin(2*np.pi*f*t+mod)*env(n,0.004,1.8)
def mallet(m,dur):
    n=int(dur*SR); t=np.arange(n)/SR; f=hz(m)
    return (np.sin(2*np.pi*f*t)+0.3*np.sin(2*np.pi*f*4*t)*np.exp(-t/0.05))*env(n,0.002,0.35)
def flute(m,dur):
    n=int(dur*SR); t=np.arange(n)/SR; f=hz(m)
    vib=0.004*np.sin(2*np.pi*5*t)*np.clip(t/0.4,0,1)
    ph=2*np.cumsum(np.pi*f*(1+vib))/SR
    x=np.sin(ph)+0.25*np.sin(2*ph)+0.08*np.sin(3*ph)+0.04*lp(rng.normal(0,1,n),3000)
    e=np.clip(t/0.15,0,1)*np.clip((dur-t)/0.3,0,1)
    return x*e
def pad(ms,dur,cut=1200):
    n=int(dur*SR); t=np.arange(n)/SR; x=np.zeros(n)
    for m in ms:
        for d in (-0.08,0,0.07):
            f=hz(m)*2**(d/12); x+=2*((t*f+rng.random())%1)-1
    x=lp(x,cut,2)/ (len(ms)*3)
    e=np.clip(t/(dur*0.35),0,1)*np.clip((dur-t)/(dur*0.35),0,1)
    return x*e
def drone(m,n):
    t=np.arange(n)/SR; f=hz(m)
    x=sum(np.sin(2*np.pi*f*k*t+k)/k**1.3 for k in range(1,7))
    return x*(0.8+0.2*np.sin(2*np.pi*t/ (n/SR) *2))
def kick(dur=0.4,f0=110,f1=45):
    n=int(dur*SR); t=np.arange(n)/SR; f=f1+(f0-f1)*np.exp(-t/0.04)
    return np.sin(2*np.pi*np.cumsum(f)/SR)*env(n,0.001,0.12)
def drum(dur,f,dec,noise=0.3):
    n=int(dur*SR); t=np.arange(n)/SR
    return (np.sin(2*np.pi*f*t*(1+0.5*np.exp(-t/0.02)))*env(n,0.001,dec)+noise*bp(rng.normal(0,1,n),800,5000)*env(n,0.001,dec*0.3))
def shaker(dur=0.12,amp=1):
    n=int(dur*SR); return hp(rng.normal(0,1,n),5000)*env(n,0.01,0.03)*amp
def hat(): n=int(0.08*SR); return hp(rng.normal(0,1,n),8000)*env(n,0.001,0.02)

class Track:
    def __init__(s,bpm,bars,beats=4):
        s.beat=60/bpm; s.N=int(round(bars*beats*s.beat*SR)); s.L=np.zeros((2,s.N+SR*8))
    def add(s,x,beat,pan=0.0,g=1.0):
        i=int(round(beat*s.beat*SR)); x=x*g
        l=np.cos((pan+1)*np.pi/4); r=np.sin((pan+1)*np.pi/4)
        e=min(len(x),s.L.shape[1]-i)
        s.L[0,i:i+e]+=x[:e]*l*1.41; s.L[1,i:i+e]+=x[:e]*r*1.41
    def render(s,rev=0.3,rlen=3.0):
        n=int(rlen*SR); t=np.arange(n)/SR
        ir=[rng.normal(0,1,n)*np.exp(-t/(rlen/5)) for _ in range(2)]
        ir=[lp(i,6000) for i in ir]
        wet=np.array([fftconvolve(s.L[c],ir[c])[:s.L.shape[1]] for c in range(2)])
        wet*=np.sqrt(np.mean(s.L**2))/max(1e-9,np.sqrt(np.mean(wet**2)))
        y=s.L+rev*wet
        out=y[:,:s.N].copy()
        tail=y[:,s.N:]; k=min(tail.shape[1],s.N); out[:,:k]+=tail[:,:k]   # wrap the tail for a seamless loop
        out=hp(out,35)
        out/=np.max(np.abs(out))/0.7
        return out
