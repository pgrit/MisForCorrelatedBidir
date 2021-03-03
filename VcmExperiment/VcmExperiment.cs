using System.Collections.Generic;
using SeeSharp.Integrators;
using SeeSharp.Integrators.Bidir;
using MisForCorrelatedBidir.Common;
using System;
using SeeSharp.Image;
using SeeSharp;

namespace MisForCorrelatedBidir.VcmExperiment {
    class VcmExperiment : SeeSharp.Experiments.ExperimentFactory {
        public override FrameBuffer.Flags FrameBufferFlags => FrameBuffer.Flags.SendToTev;
        public SceneConfig Scene { get; init; }

        protected virtual int Samples => 4;
        protected virtual int PrepassSamples => 16;

        public VcmExperiment(SceneConfig scene, bool runQuickTest=false) {
            Scene = scene;
            this.runQuickTest = runQuickTest;
        }
        bool runQuickTest;

        public override List<Method> MakeMethods() {
            var varAwarePrepass = new VarAwareMisVcm() {
                MaxDepth = Scene.MaxDepth, NumIterations = PrepassSamples, MergePrimary = false,
                RenderTechniquePyramid = false
            };

            List<Method> result = new() {
                new Method("VarAwareMisLive", new VarAwareMisVcm() {
                    MaxDepth = Scene.MaxDepth, NumIterations = Samples, MergePrimary = false,
                    RenderTechniquePyramid = false, Prepass = null
                }),

                new Method("Vcm", new VertexConnectionAndMerging() {
                    MaxDepth = Scene.MaxDepth, NumIterations = Samples, MergePrimary = false,
                    RenderTechniquePyramid = false
                }),

                new Method("PdfRatioFov", new PdfRatioVcm() {
                    MaxDepth = Scene.MaxDepth, NumIterations = Samples, MergePrimary = false,
                    RenderTechniquePyramid = false,
                    RadiusInitializer = new RadiusInitFov { ScalingFactor = MathF.Tan(MathF.PI / 180) }
                }),
                new Method("PdfRatioScene", new PdfRatioVcm() {
                    MaxDepth = Scene.MaxDepth, NumIterations = Samples, MergePrimary = false,
                    RenderTechniquePyramid = false,
                    RadiusInitializer = new RadiusInitScene { ScalingFactor = 0.01f }
                }),
                new Method("PdfRatioPixel", new PdfRatioVcm() {
                    MaxDepth = Scene.MaxDepth, NumIterations = Samples, MergePrimary = false,
                    RenderTechniquePyramid = false,
                    RadiusInitializer = new RadiusInitPixel { ScalingFactor = 50 }
                }),
                new Method("PdfRatioCombined", new PdfRatioVcm() {
                    MaxDepth = Scene.MaxDepth, NumIterations = Samples, MergePrimary = false,
                    RenderTechniquePyramid = false,
                    RadiusInitializer = new RadiusInitCombined {
                        Candidates = new() {
                            new RadiusInitFov { ScalingFactor = MathF.Tan(MathF.PI / 180) },
                            new RadiusInitScene { ScalingFactor = 0.02f }
                        }
                    }
                }),

                new Method("JendersieFootprint", new JendersieFootprint() {
                    MaxDepth = Scene.MaxDepth, NumIterations = Samples, MergePrimary = false,
                    RenderTechniquePyramid = false
                }),
            };

            if (!runQuickTest) {
                result.AddRange(new List<Method>() {
                    new Method("PathTracer", new PathTracer() {
                        MaxDepth = (uint)Scene.MaxDepth, TotalSpp = Samples * 2,
                    }),
                    new Method("Bidir", new SeeSharp.Integrators.Bidir.ClassicBidir() {
                        MaxDepth = Scene.MaxDepth, NumIterations = Samples,
                        RenderTechniquePyramid = false
                    }),
                    new Method("VarAwarePrepass", varAwarePrepass),
                    new Method("VarAwareMis", new VarAwareMisVcm() {
                        MaxDepth = Scene.MaxDepth, NumIterations = Samples, MergePrimary = false,
                        RenderTechniquePyramid = false, Prepass = varAwarePrepass
                    }),
                    new Method("UpperBound", new PdfRatioVcm() {
                        MaxDepth = Scene.MaxDepth, NumIterations = Samples, MergePrimary = false,
                        RenderTechniquePyramid = false, UseUpperBound = true
                    }),
                });
            }

            return result;
        }

        public override Scene MakeScene() => Scene.MakeScene();
        public override Integrator MakeReferenceIntegrator() => Scene.MakeReferenceIntegrator();
    }
}