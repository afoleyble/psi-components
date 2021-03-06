﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Psi;
using Microsoft.Psi.Audio;
using PipelineControl;
using GoogleASRComponent;
using GoogleSpeakComponent;
using GoogleTranslateComponent;
using AudioOutputComponent;
using ActiveMQComponent;

namespace PsiComponents
{
    class SpeechTranslate
    {
        static void Main(string[] args)
        {
            using (Pipeline pipeline = Pipeline.Create())
            {

                WaveFormat waveFormat = WaveFormat.Create16kHz1Channel16BitPcm();

                IProducer<AudioBuffer> audioInput = new AudioCapture(pipeline, new AudioCaptureConfiguration() { OutputFormat = waveFormat });
                DataFaucet<AudioBuffer> df = new DataFaucet<AudioBuffer>(pipeline);
                audioInput.PipeTo(df);
                AggregateDump dump = new AggregateDump(pipeline);
                df.PipeTo(dump);
                GoogleASR gsr = new GoogleASR(pipeline, "en"); //gsr for google speech recognition
                dump.PipeTo(gsr);
                GoogleTranslate gt = new GoogleTranslate(pipeline, "en", "de"); //gt for google translate
                gsr.PipeTo(gt);
                GoogleSpeak gs = new GoogleSpeak(pipeline, waveFormat, "de-DE"); //gs for google speak
                gt.PipeTo(gs);
                AudioOutput aOut = new AudioOutput(pipeline); //aOut for audio out
                gs.PipeTo(aOut);

                ActiveMQ rasa = new ActiveMQ(pipeline, "rasa.PSI", "rasa.PYTHON");
                gsr.PipeTo(rasa);

                GUI gui = new GUI(df, dump, gsr, gt);
                Thread thread = new Thread(() =>
                {
                    gui.ShowDialog();
                });
                thread.Start();

                pipeline.RunAsync();

                Console.ReadKey(true);
            }
            }
    }
}
