using System;
using System.Numerics;
using SeeSharp.Core;
using SeeSharp.Core.Shading;
using SeeSharp.Integrators;
using SeeSharp.Integrators.Bidir;
using SeeSharp.Integrators.Common;

namespace MisForCorrelatedBidir.Common {
    public class VarAwareMisVcm : VertexConnectionAndMerging {
        MergeVarianceFactors varianceFactors;

        public int NumTrainingSamples = 1;
        public bool ResetAfterTraining = false;
        public VarAwareMisVcm Prepass;

        public override void RegisterSample(ColorRGB weight, float misWeight, Vector2 pixel,
                                            int cameraPathLength, int lightPathLength, int fullLength) {
            base.RegisterSample(weight, misWeight, pixel, cameraPathLength, lightPathLength, fullLength);
            varianceFactors.Add(cameraPathLength, lightPathLength, fullLength, pixel, weight);
        }

        public override void PostIteration(uint iteration) {
            varianceFactors.EndIteration();
            if (ResetAfterTraining && iteration == NumTrainingSamples - 1) {
                scene.FrameBuffer.Reset();
            }
        }

        public override void PreIteration(uint iteration) {
            varianceFactors.StartIteration();
        }

        public override void Render(Scene scene) {
            if (Prepass != null) // Allow user to pass results from a prepass instead
                varianceFactors = Prepass.varianceFactors;
            else {
                if (NumLightPaths == 0) NumLightPaths = scene.FrameBuffer.Width * scene.FrameBuffer.Height;
                varianceFactors = new MergeVarianceFactors(MaxDepth, scene.FrameBuffer.Width,
                    scene.FrameBuffer.Height, NumLightPaths);
            }
            base.Render(scene);

            if (RenderTechniquePyramid) {
                string path = System.IO.Path.Join(scene.FrameBuffer.Basename, "variance-factors");
                varianceFactors.WriteToFiles(path);
            }
        }

        public override float MergeMis(CameraPath cameraPath, PathVertex lightVertex, float pdfCameraReverse,
                                       float pdfLightReverse, float pdfNextEvent) {
            int numPdfs = cameraPath.Vertices.Count + lightVertex.Depth;
            int lastCameraVertexIdx = cameraPath.Vertices.Count - 1;

            var pathPdfs = new BidirPathPdfs(lightPaths.PathCache, numPdfs);
            pathPdfs.GatherCameraPdfs(cameraPath, lastCameraVertexIdx);
            pathPdfs.GatherLightPdfs(lightVertex, lastCameraVertexIdx - 1, numPdfs);

            // Set the pdf values that are unique to this combination of paths
            if (lastCameraVertexIdx > 0) // only if this is not the primary hit point
                pathPdfs.PdfsLightToCamera[lastCameraVertexIdx - 1] = pdfCameraReverse;
            pathPdfs.PdfsLightToCamera[lastCameraVertexIdx] = lightVertex.PdfFromAncestor;
            pathPdfs.PdfsCameraToLight[lastCameraVertexIdx] = cameraPath.Vertices[^1].PdfFromAncestor;
            pathPdfs.PdfsCameraToLight[lastCameraVertexIdx + 1] = pdfLightReverse + pdfNextEvent;

            // Compute the acceptance probability approximation
            float mergeApproximation =
                pathPdfs.PdfsLightToCamera[lastCameraVertexIdx] * MathF.PI * Radius * Radius * NumLightPaths;

            // Compute the variance factor for this merge
            mergeApproximation *= varianceFactors.Get(cameraPath.Vertices.Count, numPdfs, cameraPath.Pixel);

            // Compute reciprocals for hypothetical connections along the camera sub-path
            float sumReciprocals = 0.0f;
            sumReciprocals += CameraPathReciprocals(lastCameraVertexIdx, numPdfs, pathPdfs, cameraPath.Pixel)
                / mergeApproximation;
            sumReciprocals += LightPathReciprocals(lastCameraVertexIdx, numPdfs, pathPdfs, cameraPath.Pixel)
                / mergeApproximation;

            // Add the reciprocal for the connection that replaces the last light path edge
            if (lightVertex.Depth > 1 && EnableConnections)
                sumReciprocals += 1 / mergeApproximation;

            return 1 / sumReciprocals;
        }

        protected override float CameraPathReciprocals(int lastCameraVertexIdx, int numPdfs,
                                                       BidirPathPdfs pdfs, Vector2 pixel) {
            float sumReciprocals = 0.0f;
            float nextReciprocal = 1.0f;
            for (int i = lastCameraVertexIdx; i > 0; --i) {
                // Merging at this vertex
                float acceptProb = pdfs.PdfsLightToCamera[i] * MathF.PI * Radius * Radius;
                acceptProb *= varianceFactors.Get(i + 1, numPdfs, pixel);
                sumReciprocals += nextReciprocal * NumLightPaths * acceptProb;

                nextReciprocal *= pdfs.PdfsLightToCamera[i] / pdfs.PdfsCameraToLight[i];

                // Connecting this vertex to the next one along the camera path
                if (EnableConnections) sumReciprocals += nextReciprocal;
            }

            // Light tracer
            if (EnableLightTracing)
                sumReciprocals +=
                    nextReciprocal * pdfs.PdfsLightToCamera[0] / pdfs.PdfsCameraToLight[0] * NumLightPaths;

            // Merging directly visible (almost the same as the light tracer!)
            if (MergePrimary)
                sumReciprocals += nextReciprocal * NumLightPaths * pdfs.PdfsLightToCamera[0]
                                * MathF.PI * Radius * Radius;

            return sumReciprocals;
        }

        protected override float LightPathReciprocals(int lastCameraVertexIdx, int numPdfs,
                                                      BidirPathPdfs pdfs, Vector2 pixel) {
            float sumReciprocals = 0.0f;
            float nextReciprocal = 1.0f;
            for (int i = lastCameraVertexIdx + 1; i < numPdfs; ++i) {
                if (i < numPdfs - 1 && (MergePrimary || i > 0)) { // no merging on the emitter itself
                    // Account for merging at this vertex
                    float acceptProb = pdfs.PdfsCameraToLight[i] * MathF.PI * Radius * Radius;
                    acceptProb *= varianceFactors.Get(i + 1, numPdfs, pixel);
                    sumReciprocals += nextReciprocal * NumLightPaths * acceptProb;
                }

                nextReciprocal *= pdfs.PdfsCameraToLight[i] / pdfs.PdfsLightToCamera[i];

                // Account for connections from this vertex to its ancestor
                if (i < numPdfs - 2) // Connections to the emitter (next event) are treated separately
                    if (EnableConnections) sumReciprocals += nextReciprocal;
            }
            // Next event and hitting the emitter directly
            if (EnableNextEvent || EnableBsdfLightHit) sumReciprocals += nextReciprocal;
            return sumReciprocals;
        }
    }
}