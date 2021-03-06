using System;
using System.Numerics;
using SeeSharp.Integrators;
using SeeSharp.Integrators.Bidir;
using SeeSharp.Integrators.Common;

namespace MisForCorrelatedBidir.Common {
    public class JendersieFootprint : VertexConnectionAndMerging {
        public readonly ref struct PdfRatio {
            readonly Span<float> cameraToLight;
            readonly Span<float> lightToCamera;
            readonly JendersieFootprint parent;
            readonly int numPaths;
            public PdfRatio(BidirPathPdfs pdfs, float radius, int numPaths, JendersieFootprint parent) {
                this.numPaths = numPaths;
                this.parent = parent;
                int numSurfaceVertices = pdfs.PdfsCameraToLight.Length - 1;
                cameraToLight = new float[numSurfaceVertices];
                lightToCamera = new float[numSurfaceVertices];

                float offset = 0.01f;

                // gather camera "footprint"
                float sum = 1 / (offset + MathF.Sqrt(pdfs.PdfsCameraToLight[0]));
                cameraToLight[0] = sum;
                for (int i = 1; i < numSurfaceVertices; ++i) {
                    float next = pdfs.PdfsCameraToLight[i];
                    sum += 1 / (offset + MathF.Sqrt(next));
                    cameraToLight[i] = sum;
                }

                // gather light "footprint"
                sum = 1 / (offset + MathF.Sqrt(pdfs.PdfsLightToCamera[numSurfaceVertices - 1]));
                lightToCamera[numSurfaceVertices - 1] = sum;
                for (int i = numSurfaceVertices - 2; i >= 0; --i) {
                    float next = pdfs.PdfsLightToCamera[i];
                    sum += 1 / (offset + MathF.Sqrt(next));
                    lightToCamera[i] = sum;
                }
            }

            public float this[int idx] {
                get {
                    float light = lightToCamera[idx];
                    float cam = cameraToLight[idx];

                    float parameter = 0.5f;
                    cam = MathF.Pow(cam, 4 + parameter);
                    light = MathF.Pow(light, 4 + parameter);
                    float ratio = (cam + light) / (numPaths * cam + light);

                    return ratio;
                }
            }
        }

        public override float MergeMis(CameraPath cameraPath, PathVertex lightVertex, float pdfCameraReverse,
                                       float pdfLightReverse, float pdfNextEvent) {
            int numPdfs = cameraPath.Vertices.Count + lightVertex.Depth;
            int lastCameraVertexIdx = cameraPath.Vertices.Count - 1;
            Span<float> camToLight = stackalloc float[numPdfs];
            Span<float> lightToCam = stackalloc float[numPdfs];
            var pathPdfs = new BidirPathPdfs(lightPaths.PathCache, lightToCam, camToLight);
            pathPdfs.GatherCameraPdfs(cameraPath, lastCameraVertexIdx);
            pathPdfs.GatherLightPdfs(lightVertex, lastCameraVertexIdx - 1, numPdfs);

            // Set the pdf values that are unique to this combination of paths
            if (lastCameraVertexIdx > 0) // only if this is not the primary hit point
                pathPdfs.PdfsLightToCamera[lastCameraVertexIdx - 1] = pdfCameraReverse;
            pathPdfs.PdfsLightToCamera[lastCameraVertexIdx] = lightVertex.PdfFromAncestor;
            pathPdfs.PdfsCameraToLight[lastCameraVertexIdx] = cameraPath.Vertices[^1].PdfFromAncestor;
            pathPdfs.PdfsCameraToLight[lastCameraVertexIdx + 1] = pdfLightReverse + pdfNextEvent;

            // Compute our additional heuristic values
            var diffRatio = new PdfRatio(pathPdfs, Radius, NumLightPaths, this);

            // Compute the acceptance probability approximation
            float mergeApproximation = pathPdfs.PdfsLightToCamera[lastCameraVertexIdx]
                                     * MathF.PI * Radius * Radius * NumLightPaths;
            mergeApproximation *= diffRatio[lastCameraVertexIdx];

            if (mergeApproximation == 0.0f) return 0.0f;

            // Compute reciprocals for hypothetical connections along the camera sub-path
            float sumReciprocals = 0.0f;
            sumReciprocals +=
                CameraPathReciprocals(lastCameraVertexIdx, numPdfs, pathPdfs, cameraPath.Pixel, diffRatio)
                / mergeApproximation;
            sumReciprocals +=
                LightPathReciprocals(lastCameraVertexIdx, numPdfs, pathPdfs, cameraPath.Pixel, diffRatio)
                / mergeApproximation;

            // Add the reciprocal for the connection that replaces the last light path edge
            if (lightVertex.Depth > 1)
                if (EnableConnections) sumReciprocals += 1 / mergeApproximation;

            return 1 / sumReciprocals;
        }

        public override float EmitterHitMis(CameraPath cameraPath, float pdfEmit, float pdfNextEvent) {
            int numPdfs = cameraPath.Vertices.Count;
            int lastCameraVertexIdx = numPdfs - 1;

            if (numPdfs == 1) return 1.0f; // sole technique for rendering directly visible lights.

            // Gather the pdfs
            Span<float> camToLight = stackalloc float[numPdfs];
            Span<float> lightToCam = stackalloc float[numPdfs];
            var pathPdfs = new BidirPathPdfs(lightPaths.PathCache, lightToCam, camToLight);
            pathPdfs.GatherCameraPdfs(cameraPath, lastCameraVertexIdx);
            pathPdfs.PdfsLightToCamera[^2] = pdfEmit;

            // Compute our additional heuristic values
            var diffRatio = new PdfRatio(pathPdfs, Radius, NumLightPaths, this);

            float pdfThis = cameraPath.Vertices[^1].PdfFromAncestor;

            // Compute the actual weight
            float sumReciprocals = 1.0f;

            // Next event estimation
            if (EnableNextEvent) sumReciprocals += pdfNextEvent / pdfThis;

            // All connections along the camera path
            sumReciprocals +=
                CameraPathReciprocals(lastCameraVertexIdx - 1, numPdfs, pathPdfs, cameraPath.Pixel, diffRatio)
                / pdfThis;

            return 1 / sumReciprocals;
        }

        public override float LightTracerMis(PathVertex lightVertex, float pdfCamToPrimary, float pdfReverse,
                                             float pdfNextEvent, Vector2 pixel, float distToCam) {
            int numPdfs = lightVertex.Depth + 1;
            int lastCameraVertexIdx = -1;
            Span<float> camToLight = stackalloc float[numPdfs];
            Span<float> lightToCam = stackalloc float[numPdfs];
            var pathPdfs = new BidirPathPdfs(lightPaths.PathCache, lightToCam, camToLight);
            pathPdfs.GatherLightPdfs(lightVertex, lastCameraVertexIdx, numPdfs);
            pathPdfs.PdfsCameraToLight[0] = pdfCamToPrimary;
            pathPdfs.PdfsCameraToLight[1] = pdfReverse + pdfNextEvent;

            // Compute our additional heuristic values
            var diffRatio = new PdfRatio(pathPdfs, Radius, NumLightPaths, this);

            // Compute the actual weight
            float sumReciprocals = LightPathReciprocals(lastCameraVertexIdx, numPdfs, pathPdfs, pixel, diffRatio);
            sumReciprocals /= NumLightPaths;
            sumReciprocals += 1;

            return 1 / sumReciprocals;
        }

        public override float BidirConnectMis(CameraPath cameraPath, PathVertex lightVertex, float pdfCameraReverse,
                                              float pdfCameraToLight, float pdfLightReverse, float pdfLightToCamera,
                                              float pdfNextEvent) {
            int numPdfs = cameraPath.Vertices.Count + lightVertex.Depth + 1;
            int lastCameraVertexIdx = cameraPath.Vertices.Count - 1;
            Span<float> camToLight = stackalloc float[numPdfs];
            Span<float> lightToCam = stackalloc float[numPdfs];
            var pathPdfs = new BidirPathPdfs(lightPaths.PathCache, lightToCam, camToLight);
            pathPdfs.GatherCameraPdfs(cameraPath, lastCameraVertexIdx);
            pathPdfs.GatherLightPdfs(lightVertex, lastCameraVertexIdx, numPdfs);

            // Set the pdf values that are unique to this combination of paths
            if (lastCameraVertexIdx > 0) // only if this is not the primary hit point
                pathPdfs.PdfsLightToCamera[lastCameraVertexIdx - 1] = pdfCameraReverse;
            pathPdfs.PdfsCameraToLight[lastCameraVertexIdx] = cameraPath.Vertices[^1].PdfFromAncestor;
            pathPdfs.PdfsLightToCamera[lastCameraVertexIdx] = pdfLightToCamera;
            pathPdfs.PdfsCameraToLight[lastCameraVertexIdx + 1] = pdfCameraToLight;
            pathPdfs.PdfsCameraToLight[lastCameraVertexIdx + 2] = pdfLightReverse + pdfNextEvent;

            // Compute our additional heuristic values
            var diffRatio = new PdfRatio(pathPdfs, Radius, NumLightPaths, this);

            // Compute reciprocals for hypothetical connections along the camera sub-path
            float sumReciprocals = 1.0f;
            sumReciprocals += CameraPathReciprocals(lastCameraVertexIdx, numPdfs, pathPdfs, cameraPath.Pixel, diffRatio);
            sumReciprocals += LightPathReciprocals(lastCameraVertexIdx, numPdfs, pathPdfs, cameraPath.Pixel, diffRatio);

            return 1 / sumReciprocals;
        }

        public override float NextEventMis(CameraPath cameraPath, float pdfEmit, float pdfNextEvent,
                                           float pdfHit, float pdfReverse) {
            int numPdfs = cameraPath.Vertices.Count + 1;
            int lastCameraVertexIdx = numPdfs - 2;
            Span<float> camToLight = stackalloc float[numPdfs];
            Span<float> lightToCam = stackalloc float[numPdfs];
            var pathPdfs = new BidirPathPdfs(lightPaths.PathCache, lightToCam, camToLight);
            pathPdfs.GatherCameraPdfs(cameraPath, lastCameraVertexIdx);
            pathPdfs.PdfsCameraToLight[^2] = cameraPath.Vertices[^1].PdfFromAncestor;
            pathPdfs.PdfsLightToCamera[^2] = pdfEmit;
            if (numPdfs > 2) // not for direct illumination
                pathPdfs.PdfsLightToCamera[^3] = pdfReverse;

            // Compute our additional heuristic values
            var diffRatio = new PdfRatio(pathPdfs, Radius, NumLightPaths, this);

            // Compute the actual weight
            float sumReciprocals = 1.0f;

            // Hitting the light source
            if (EnableBsdfLightHit) sumReciprocals += pdfHit / pdfNextEvent;

            // All bidirectional connections
            sumReciprocals += CameraPathReciprocals(lastCameraVertexIdx, numPdfs, pathPdfs, cameraPath.Pixel,
                diffRatio) / pdfNextEvent;

            return 1 / sumReciprocals;
        }

        protected float CameraPathReciprocals(int lastCameraVertexIdx, int numPdfs, BidirPathPdfs pdfs,
                                              Vector2 pixel, PdfRatio diffRatio) {
            float sumReciprocals = 0.0f;
            float nextReciprocal = 1.0f;
            for (int i = lastCameraVertexIdx; i > 0; --i) {
                // Merging at this vertex
                float acceptProb = pdfs.PdfsLightToCamera[i] * MathF.PI * Radius * Radius;
                acceptProb *= diffRatio[i];
                sumReciprocals += nextReciprocal * NumLightPaths * acceptProb;

                nextReciprocal *= pdfs.PdfsLightToCamera[i] / pdfs.PdfsCameraToLight[i];

                // Connecting this vertex to the next one along the camera path
                if (EnableConnections) sumReciprocals += nextReciprocal;
            }

            // Light tracer
            if (EnableLightTracing)
                sumReciprocals += nextReciprocal * pdfs.PdfsLightToCamera[0] / pdfs.PdfsCameraToLight[0]
                    * NumLightPaths;

            // Merging directly visible (almost the same as the light tracer!)
            if (MergePrimary)
                sumReciprocals += nextReciprocal * NumLightPaths * pdfs.PdfsLightToCamera[0]
                                * MathF.PI * Radius * Radius;

            return sumReciprocals;
        }

        protected float LightPathReciprocals(int lastCameraVertexIdx, int numPdfs, BidirPathPdfs pdfs,
                                             Vector2 pixel, PdfRatio diffRatio) {
            float sumReciprocals = 0.0f;
            float nextReciprocal = 1.0f;
            for (int i = lastCameraVertexIdx + 1; i < numPdfs; ++i) {
                if (i < numPdfs - 1 && (MergePrimary || i > 0)) { // no merging on the emitter itself
                    // Account for merging at this vertex
                    float acceptProb = pdfs.PdfsCameraToLight[i] * MathF.PI * Radius * Radius;
                    acceptProb *= diffRatio[i];
                    sumReciprocals += nextReciprocal * NumLightPaths * acceptProb;
                }

                nextReciprocal *= pdfs.PdfsCameraToLight[i] / pdfs.PdfsLightToCamera[i];

                // Account for connections from this vertex to its ancestor
                if (i < numPdfs - 2) // Connections to the emitter (next event) are treated separately
                    if (EnableConnections) sumReciprocals += nextReciprocal;
            }
            if (EnableBsdfLightHit || EnableNextEvent) sumReciprocals += nextReciprocal; // Next event and hitting the emitter directly
            return sumReciprocals;
        }
    }
}