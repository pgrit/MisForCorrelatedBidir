using System.Collections.Generic;
using SeeSharp.Integrators;
using SeeSharp.Integrators.Bidir;
using MisForCorrelatedBidir.Common;
using System;

namespace MisForCorrelatedBidir.VcmExperiment {
    class VcmExperiment : SeeSharp.Experiments.Experiment {
        bool runQuickTest;
        int samples;
        int prepassSamples;

        public VcmExperiment(bool runQuickTest = false, int samples = 4, int prepassSamples = 16) {
            this.runQuickTest = runQuickTest;
            this.samples = samples;
            this.prepassSamples = prepassSamples;
        }

        public override List<Method> MakeMethods() {
            var varAwarePrepass = new VarAwareMisVcm() {
                NumIterations = prepassSamples, MergePrimary = false,
                RenderTechniquePyramid = false
            };

            List<Method> result = new() {
                new Method("VarAwareMisLive", new VarAwareMisVcm() {
                    NumIterations = samples, MergePrimary = false,
                    RenderTechniquePyramid = false, Prepass = null
                }),

                new Method("Vcm", new VertexConnectionAndMerging() {
                    NumIterations = samples, MergePrimary = false,
                    RenderTechniquePyramid = false
                }),

                new Method("PdfRatioFov", new PdfRatioVcm() {
                    NumIterations = samples, MergePrimary = false,
                    RenderTechniquePyramid = false,
                    RadiusInitializer = new RadiusInitFov { ScalingFactor = MathF.Tan(MathF.PI / 180) }
                }),
                new Method("PdfRatioScene", new PdfRatioVcm() {
                    NumIterations = samples, MergePrimary = false,
                    RenderTechniquePyramid = false,
                    RadiusInitializer = new RadiusInitScene { ScalingFactor = 0.01f }
                }),
                new Method("PdfRatioPixel", new PdfRatioVcm() {
                    NumIterations = samples, MergePrimary = false,
                    RenderTechniquePyramid = false,
                    RadiusInitializer = new RadiusInitPixel { ScalingFactor = 50 }
                }),
                new Method("PdfRatioCombined", new PdfRatioVcm() {
                    NumIterations = samples, MergePrimary = false,
                    RenderTechniquePyramid = false,
                    RadiusInitializer = new RadiusInitCombined {
                        Candidates = new() {
                            new RadiusInitFov { ScalingFactor = MathF.Tan(MathF.PI / 180) },
                            new RadiusInitScene { ScalingFactor = 0.02f }
                        }
                    }
                }),

                new Method("JendersieFootprint", new JendersieFootprint() {
                    NumIterations = samples, MergePrimary = false,
                    RenderTechniquePyramid = false
                }),
            };

            if (!runQuickTest) {
                result.AddRange(new List<Method>() {
                    new Method("PathTracer", new PathTracer() {
                        TotalSpp = samples * 2,
                    }),
                    new Method("Bidir", new SeeSharp.Integrators.Bidir.ClassicBidir() {
                        NumIterations = samples,
                        RenderTechniquePyramid = false
                    }),
                    new Method("VarAwarePrepass", varAwarePrepass),
                    new Method("VarAwareMis", new VarAwareMisVcm() {
                        NumIterations = samples, MergePrimary = false,
                        RenderTechniquePyramid = false, Prepass = varAwarePrepass
                    }),
                    new Method("UpperBound", new PdfRatioVcm() {
                        NumIterations = samples, MergePrimary = false,
                        RenderTechniquePyramid = false, UseUpperBound = true
                    }),
                });
            }

            return result;
        }
    }
}