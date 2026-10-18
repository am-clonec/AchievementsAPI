using System;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace AchievementsAPI;

/// <summary>
/// A utility class for loading .WAV audio assets from embedded resources.
/// </summary>
public static class WavLoader
{
    /// <summary>
    /// Loads a PCM .WAV file from an embedded resource into an <see cref="AudioClip"/>.
    /// </summary>
    /// <param name="assembly">The assembly containing the embedded resource.</param>
    /// <param name="resourcePath">The manifest resource name of the WAV file.</param>
    /// <param name="fadeOutSeconds">Length of a smooth fade applied to the end of the clip; 0 for none.</param>
    /// <returns>The loaded <see cref="AudioClip"/>, or null if loading failed.</returns>
    public static AudioClip? LoadFromResource(Assembly assembly, string resourcePath, float fadeOutSeconds = 0.6f)
    {
        try
        {
            using var assetStream = assembly.GetManifestResourceStream(resourcePath);
            if (assetStream == null)
            {
                AchievementsAPIPlugin.Instance.Log.LogWarning($"Stream failed to load. Check if the asset name is correct: {resourcePath}");
                return null;
            }

            using var reader = new BinaryReader(assetStream);

            // Read RIFF header
            if (Encoding.ASCII.GetString(reader.ReadBytes(4)) != "RIFF")
            {
                AchievementsAPIPlugin.Instance.Log.LogWarning($"Invalid WAV file (missing RIFF header): {resourcePath}");
                return null;
            }

            reader.ReadInt32(); // File size (ignore)

            if (Encoding.ASCII.GetString(reader.ReadBytes(4)) != "WAVE")
            {
                AchievementsAPIPlugin.Instance.Log.LogWarning($"Invalid WAV file (missing WAVE header): {resourcePath}");
                return null;
            }

            // Read chunks
            int channels = 0, sampleRate = 0, bitsPerSample = 0;
            byte[]? audioData = null;

            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                var chunkId = Encoding.ASCII.GetString(reader.ReadBytes(4));
                var chunkSize = reader.ReadInt32();

                // Format chunk
                if (chunkId == "fmt ")
                {
                    int audioFormat = reader.ReadInt16();
                    channels = reader.ReadInt16();
                    sampleRate = reader.ReadInt32();
                    reader.ReadInt32(); // Byte rate (ignore)
                    reader.ReadInt16(); // Block align (ignore)
                    bitsPerSample = reader.ReadInt16();

                    if (audioFormat != 1) // Only PCM supported
                    {
                        AchievementsAPIPlugin.Instance.Log.LogWarning($"Only PCM WAV files are supported: {resourcePath}");
                        return null;
                    }
                }

                // Data chunk
                else if (chunkId == "data")
                {
                    audioData = reader.ReadBytes(chunkSize);
                    break; // No need to read further
                }
                else
                {
                    reader.BaseStream.Seek(chunkSize, SeekOrigin.Current); // Skip unknown chunks
                }
            }

            if (audioData == null)
            {
                AchievementsAPIPlugin.Instance.Log.LogWarning($"WAV file does not contain audio data: {resourcePath}");
                return null;
            }

            if (channels == 0 || sampleRate == 0 || bitsPerSample == 0)
            {
                AchievementsAPIPlugin.Instance.Log.LogWarning($"WAV file does not contain valid format information: {resourcePath}");
                return null;
            }

            // Convert PCM data to float array
            var samples = ConvertPcmToFloat(audioData, bitsPerSample);
            FadeOut(samples, channels, Mathf.RoundToInt(fadeOutSeconds * sampleRate));

            // Create the AudioClip
            var audioClip = AudioClip.Create(resourcePath, samples.Length / channels, channels, sampleRate, false);
            audioClip.SetData(samples, 0);
            audioClip.DontUnload();

            return audioClip;
        }
        catch (Exception e)
        {
            AchievementsAPIPlugin.Instance.Log.LogWarning($"Failed to load WAV resource {resourcePath}: {e}");
            return null;
        }
    }

    // Smoothstep fade over the last `fadeFrames` frames (a frame = one sample per channel)
    private static void FadeOut(float[] samples, int channels, int fadeFrames)
    {
        var frames = samples.Length / channels;
        fadeFrames = Math.Clamp(fadeFrames, 0, frames);
        for (var f = frames - fadeFrames; f < frames; f++)
        {
            var t = (frames - 1 - f) / (float) Math.Max(fadeFrames - 1, 1);
            var gain = t * t * (3f - 2f * t);
            for (var c = 0; c < channels; c++) samples[f * channels + c] *= gain;
        }
    }

    // This is somewhat magic to me, I just copied it from elsewhere.
    private static float[] ConvertPcmToFloat(byte[] pcmData, int bitsPerSample)
    {
        var sampleCount = pcmData.Length / (bitsPerSample / 8);
        var floatData = new float[sampleCount];

        for (var i = 0; i < sampleCount; i++)
        {
            var byteIndex = i * (bitsPerSample / 8);

            switch (bitsPerSample)
            {
                case 8:
                    floatData[i] = (pcmData[byteIndex] - 128) / 128f; // 8-bit PCM is unsigned
                    break;
                case 16:
                    floatData[i] = BitConverter.ToInt16(pcmData, byteIndex) / 32768f;
                    break;
                case 24:
                    var sample24 = pcmData[byteIndex] | (pcmData[byteIndex + 1] << 8) | (pcmData[byteIndex + 2] << 16);
                    if (sample24 > 0x7FFFFF) sample24 -= 0x1000000; // Convert from unsigned to signed
                    floatData[i] = sample24 / 8388608f;
                    break;
                case 32:
                    floatData[i] = BitConverter.ToInt32(pcmData, byteIndex) / 2147483648f;
                    break;
                default:
                    throw new NotSupportedException($"Unsupported bit depth: {bitsPerSample}");
            }
        }

        return floatData;
    }
}
