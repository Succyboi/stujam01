namespace Stupid.Audio {
    public sealed class AudioQuality {
        public static readonly AudioQuality Poor = new AudioQuality(8000);
        public static readonly AudioQuality Mediocre = new AudioQuality(16000);
        public static readonly AudioQuality Fine = new AudioQuality(24000);
        public static readonly AudioQuality Decent = new AudioQuality(32000);
        public static readonly AudioQuality Great = new AudioQuality(44100);
        public static readonly AudioQuality Sublime = new AudioQuality(48000);

        private int sampleRate;

        public AudioQuality(int sampleRate) {
            this.sampleRate = sampleRate;
        }

        public static implicit operator int(AudioQuality from) => from.sampleRate;

    }
}