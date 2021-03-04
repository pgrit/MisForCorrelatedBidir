using System;
using System.Collections.Generic;
using SeeSharp;
using SeeSharp.Integrators;
using SeeSharp.Integrators.Bidir;
using MisForCorrelatedBidir.Common;
using SimpleImageIO;
using System.IO;

namespace MisForCorrelatedBidir.VideoGen {
    class ExperimentSetup : SeeSharp.Experiments.Experiment {
        readonly int samples = 4;
        readonly int trainingSamples = 128;

        public override List<Method> MakeMethods() {
            return new List<Method>() {
                new Method("Vcm", new VertexConnectionAndMerging() {
                    MinDepth = 3, MaxDepth = 3, NumIterations = samples, MergePrimary = true,
                    RenderTechniquePyramid = true, EnableLightTracing = false,
                    EnableConnections = false, EnableNextEvent = false, EnableBsdfLightHit = false
                }),
                new Method("PdfRatio", new PdfRatioVcm() {
                    MinDepth = 3, MaxDepth = 3, NumIterations = samples, MergePrimary = true,
                    RenderTechniquePyramid = true, EnableLightTracing = false,
                    EnableConnections = false, EnableNextEvent = false, EnableBsdfLightHit = false,
                    RadiusInitializer = new RadiusInitFov { ScalingFactor = MathF.Tan(MathF.PI / 180) }
                }),
                new Method("VarAwareRef", new VarAwareMisVcm() {
                    MinDepth = 3, MaxDepth = 3, NumIterations = samples + trainingSamples,
                    MergePrimary = true,
                    RenderTechniquePyramid = true, EnableLightTracing = false,
                    EnableConnections = false, EnableNextEvent = false, EnableBsdfLightHit = false,
                    NumTrainingSamples = trainingSamples, ResetAfterTraining = true
                }),
            };
        }
    }

    class Frame : SeeSharp.Experiments.SceneConfig {
        readonly int index;
        readonly Scene scene;

        public override int MaxDepth => 3;
        public override string Name => $"Frame{index:0000}";

        public Frame(int index) {
            this.index = index;
            scene = Scene.LoadFromFile($"../Scenes/Box/Animation/BoxMovingLight{index:0000}.json");
        }

        public override RgbImage GetReferenceImage(int width, int height) {
            string filename = $"../Scenes/Box/Animation/BoxMovingLight{index:0000}.exr";
            if (!File.Exists(filename)){
                Scene scn = MakeScene();
                scn.FrameBuffer = new(width, height, filename);
                scn.Prepare();
                ReferenceIntegrator.Render(scn);
                scn.FrameBuffer.WriteToFile();
                return scn.FrameBuffer.Image;
            }
            return new RgbImage(filename);
        }

        public Integrator ReferenceIntegrator
        => new ClassicBidir() { MinDepth = 3, MaxDepth = 3, NumIterations = 128 };

        public override Scene MakeScene()
        => scene.Copy();
    }
}