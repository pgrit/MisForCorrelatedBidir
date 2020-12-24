using System;
using System.Numerics;
using SeeSharp.Integrators;
using SeeSharp.Integrators.Common;

namespace MisForCorrelatedBidir.Common {
    public class PdfRatioBidir : SeeSharp.Integrators.Bidir.ClassicBidir {
        public RadiusInitializer RadiusInitializer;

        public float ComputeNeeFactor(BidirPathPdfs pdfs, PathVertex? lightVertex, int numPdfs,
                                      int lastCameraVertexIdx, float pdfNextEvent, float distToCam) {
            if (numPdfs <= 2) {
                return 1; // Direct illumination has no correlation
            }

            // Set a radius for the probability approximations.
            float radius = RadiusInitializer.ComputeRadius(scene.Radius, pdfs.PdfsCameraToLight[0], distToCam);
            float acceptArea = radius * radius * MathF.PI;

            // Compute the camera path determinism up until the last vertex
            int numSurfaceVertices = pdfs.PdfsCameraToLight.Length - 1;
            float cameraProbability = 1.0f;
            for (int i = 0; i < numSurfaceVertices; ++i) {
                float next = pdfs.PdfsCameraToLight[i] * acceptArea;
                next = MathF.Min(next, 1.0f);
                cameraProbability *= next;
            }

            // Retrieve the next event pdf from the second to last light path vertex (if it exists)
            while (lightVertex?.Depth > 2) {
                var ancestor = lightPaths.PathCache[lightVertex.Value.PathId, lightVertex.Value.AncestorId];
                lightVertex = ancestor;
            }
            if (lightVertex?.Depth == 2) {
                pdfNextEvent = lightVertex.Value.PdfNextEventAncestor;
            }
            float nextEventProbability = acceptArea * pdfNextEvent / NumShadowRays;
            nextEventProbability = MathF.Min(nextEventProbability, 1.0f);

            float denom = cameraProbability + nextEventProbability - cameraProbability * nextEventProbability;
            float factor = cameraProbability / denom;

            // Make sure that we only ever increase the weight, going below the provable upper bound
            // will always hurt the outcome!
            factor = MathF.Max(factor, 1.0f / NumShadowRays);

            // Compute the new joint pdf of BSDF and next event sampling
            float pdfBsdf = pdfs.PdfsCameraToLight[^1] - pdfNextEvent;
            pdfs.PdfsCameraToLight[^1] = pdfBsdf + pdfNextEvent * factor;

            return factor;
        }

        public override float EmitterHitMis(CameraPath cameraPath, float pdfEmit, float pdfNextEvent) {
            int numPdfs = cameraPath.Vertices.Count;
            int lastCameraVertexIdx = numPdfs - 1;

            if (numPdfs == 1) return 1.0f; // sole technique for rendering directly visible lights.

            var pathPdfs = new BidirPathPdfs(lightPaths.PathCache, numPdfs);
            pathPdfs.GatherCameraPdfs(cameraPath, lastCameraVertexIdx);

            pathPdfs.PdfsLightToCamera[^2] = pdfEmit;

            float factor =
                ComputeNeeFactor(pathPdfs, null, numPdfs, lastCameraVertexIdx, pdfNextEvent, cameraPath.Distances[0]);
            pdfNextEvent *= factor;

            float pdfThis = cameraPath.Vertices[^1].PdfFromAncestor;

            // Compute the actual weight
            float sumReciprocals = 1.0f;

            // Next event estimation
            sumReciprocals += pdfNextEvent / pdfThis;

            // All connections along the camera path
            sumReciprocals += CameraPathReciprocals(lastCameraVertexIdx - 1, pathPdfs) / pdfThis;

            return 1 / sumReciprocals;
        }

        public override float LightTracerMis(PathVertex lightVertex, float pdfCamToPrimary, float pdfReverse,
                                             float pdfNextEvent, Vector2 pixel, float distToCam) {
            int numPdfs = lightVertex.Depth + 1;
            int lastCameraVertexIdx = -1;

            var pathPdfs = new BidirPathPdfs(lightPaths.PathCache, numPdfs);

            pathPdfs.GatherLightPdfs(lightVertex, lastCameraVertexIdx, numPdfs);

            pathPdfs.PdfsCameraToLight[0] = pdfCamToPrimary;
            pathPdfs.PdfsCameraToLight[1] = pdfReverse + pdfNextEvent;

            ComputeNeeFactor(pathPdfs, lightVertex, numPdfs, lastCameraVertexIdx, pdfNextEvent, distToCam);

            // Compute the actual weight
            float sumReciprocals = LightPathReciprocals(lastCameraVertexIdx, numPdfs, pathPdfs);
            sumReciprocals /= NumLightPaths;
            sumReciprocals += 1;
            return 1 / sumReciprocals;
        }

        public override float BidirConnectMis(CameraPath cameraPath, PathVertex lightVertex, float pdfCameraReverse,
                                              float pdfCameraToLight, float pdfLightReverse, float pdfLightToCamera,
                                              float pdfNextEvent) {
            int numPdfs = cameraPath.Vertices.Count + lightVertex.Depth + 1;
            int lastCameraVertexIdx = cameraPath.Vertices.Count - 1;

            var pathPdfs = new BidirPathPdfs(lightPaths.PathCache, numPdfs);
            pathPdfs.GatherCameraPdfs(cameraPath, lastCameraVertexIdx);
            pathPdfs.GatherLightPdfs(lightVertex, lastCameraVertexIdx, numPdfs);

            // Set the pdf values that are unique to this combination of paths
            if (lastCameraVertexIdx > 0) // only if this is not the primary hit point
                pathPdfs.PdfsLightToCamera[lastCameraVertexIdx - 1] = pdfCameraReverse;
            pathPdfs.PdfsCameraToLight[lastCameraVertexIdx] = cameraPath.Vertices[^1].PdfFromAncestor;
            pathPdfs.PdfsLightToCamera[lastCameraVertexIdx] = pdfLightToCamera;
            pathPdfs.PdfsCameraToLight[lastCameraVertexIdx + 1] = pdfCameraToLight;
            pathPdfs.PdfsCameraToLight[lastCameraVertexIdx + 2] = pdfLightReverse + pdfNextEvent;

            ComputeNeeFactor(pathPdfs, lightVertex, numPdfs, lastCameraVertexIdx, pdfNextEvent, cameraPath.Distances[0]);

            // Compute reciprocals for hypothetical connections along the camera sub-path
            float sumReciprocals = 1.0f;
            sumReciprocals += CameraPathReciprocals(lastCameraVertexIdx, pathPdfs);
            sumReciprocals += LightPathReciprocals(lastCameraVertexIdx, numPdfs, pathPdfs);

            return 1 / sumReciprocals;
        }

        public override float NextEventMis(CameraPath cameraPath, float pdfEmit, float pdfNextEvent,
                                           float pdfHit, float pdfReverse) {
            int numPdfs = cameraPath.Vertices.Count + 1;
            int lastCameraVertexIdx = numPdfs - 2;

            var pathPdfs = new BidirPathPdfs(lightPaths.PathCache, numPdfs);

            pathPdfs.GatherCameraPdfs(cameraPath, lastCameraVertexIdx);

            pathPdfs.PdfsCameraToLight[^2] = cameraPath.Vertices[^1].PdfFromAncestor;
            pathPdfs.PdfsLightToCamera[^2] = pdfEmit;
            if (numPdfs > 2) // not for direct illumination
                pathPdfs.PdfsLightToCamera[^3] = pdfReverse;

            float factor =
                ComputeNeeFactor(pathPdfs, null, numPdfs, lastCameraVertexIdx, pdfNextEvent, cameraPath.Distances[0]);
            pdfNextEvent *= factor;

            // Compute the actual weight
            float sumReciprocals = 1.0f;

            // Hitting the light source
            sumReciprocals += pdfHit / pdfNextEvent;

            // All bidirectional connections
            sumReciprocals += CameraPathReciprocals(lastCameraVertexIdx, pathPdfs) / pdfNextEvent;

            return 1 / sumReciprocals;
        }
    }
}