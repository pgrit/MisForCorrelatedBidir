using System;
using System.Collections.Generic;
using SeeSharp;
using SeeSharp.Integrators;
using SeeSharp.Integrators.Bidir;
using SeeSharp.Image;
using MisForCorrelatedBidir.Common;

namespace MisForCorrelatedBidir.VideoGen {
    abstract class ExperimentSetup : SeeSharp.Experiments.ExperimentFactory {
        public override FrameBuffer.Flags FrameBufferFlags => FrameBuffer.Flags.SendToTev;

        protected virtual int MaxDepth => 3;
        protected virtual int MinDepth => 3;
        protected virtual int Samples => 4;

        protected virtual int TrainingSamples => 128;

        public override List<Method> MakeMethods() {
            return new List<Method>() {
                new Method("Vcm", new VertexConnectionAndMerging() {
                    MinDepth = MinDepth, MaxDepth = MaxDepth, NumIterations = Samples, MergePrimary = true,
                    RenderTechniquePyramid = true, EnableLightTracing = false,
                    EnableConnections = false, EnableNextEvent = false, EnableBsdfLightHit = false
                }),
                new Method("PdfRatio", new PdfRatioVcm() {
                    MinDepth = MinDepth, MaxDepth = MaxDepth, NumIterations = Samples, MergePrimary = true,
                    RenderTechniquePyramid = true, EnableLightTracing = false,
                    EnableConnections = false, EnableNextEvent = false, EnableBsdfLightHit = false,
                    RadiusInitializer = new RadiusInitFov { ScalingFactor = MathF.Tan(MathF.PI / 180) }
                }),
                new Method("VarAwareRef", new VarAwareMisVcm() {
                    MinDepth = MinDepth, MaxDepth = MaxDepth, NumIterations = Samples + TrainingSamples,
                    MergePrimary = true,
                    RenderTechniquePyramid = true, EnableLightTracing = false,
                    EnableConnections = false, EnableNextEvent = false, EnableBsdfLightHit = false,
                    NumTrainingSamples = TrainingSamples, ResetAfterTraining = true
                }),
            };
        }

        public override Integrator MakeReferenceIntegrator() =>
            new ClassicBidir() { MinDepth = MinDepth, MaxDepth = MaxDepth, NumIterations = 128 };
    }

    class Frame : ExperimentSetup {
        public int Index = 0;
        public Frame(int index) { Index = index; }
        public override Scene MakeScene() =>
            Scene.LoadFromFile($"../Scenes/Box/Animation/BoxMovingLight{Index:0000}.json");
    }
}