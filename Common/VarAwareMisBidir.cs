using System.Collections.Generic;
using System.Numerics;
using SeeSharp.Core;
using SeeSharp.Core.Geometry;
using SeeSharp.Core.Image;
using SeeSharp.Core.Sampling;
using SeeSharp.Core.Shading;
using SeeSharp.Core.Shading.Emitters;
using SeeSharp.Integrators;
using SeeSharp.Integrators.Bidir;
using SeeSharp.Integrators.Common;

namespace MisForCorrelatedBidir.Common {
    public class NextEventVarianceFactors {
        public NextEventVarianceFactors(int maxDepth, int width, int height) {
            this.maxDepth = maxDepth;

            moments = new(maxDepth - 2);
            pixelValues = new(maxDepth - 2);
            varianceFactors = new(maxDepth - 2);
            for (int i = 2; i < maxDepth; ++i) { // all depths with correlated next event (i.e., no DI)
                moments.Add(new(width, height));
                pixelValues.Add(new(width, height));
                varianceFactors.Add(new(width, height));
            }
        }

        public void StartIteration() {
            curIteration++;

            // Scale values of the previous iteration to account for having more samples
            if (curIteration > 1) {
                for (int i = 0; i < moments.Count; ++i) {
                    moments[i].Scale((curIteration - 1.0f) / curIteration);
                    pixelValues[i].Scale((curIteration - 1.0f) / curIteration);
                }
            }
        }

        public void EndIteration() {
            int width = moments[0].Width;
            int height = moments[0].Height;
            var filter = new BoxFilter(4);
            Image<Scalar> momentBuffer = new(width, height);
            Image<Scalar> varianceBuffer = new(width, height);

            // Compute the variance factors for use in the next iteration
            for (int i = 0; i < moments.Count; ++i) {
                // Estimate the pixel variances:
                // First, we blur the image. Then, we subtract the blurred version from the original.
                // Finally, we compute and square the difference, multiplying by the number of iterations
                // to obtain a coarse estimate of the variance in a single iteration.
                filter.Apply(pixelValues[i], varianceBuffer);
                for (int row = 0; row < height; ++row) {
                    for (int col = 0; col < width; ++col) {
                        var delta = pixelValues[i][col, row].Value - varianceBuffer[col, row].Value;
                        var mean = varianceBuffer[col, row];

                        var variance = delta * delta * curIteration;
                        varianceBuffer[col, row] = new(variance);
                    }
                }
                filter.Apply(varianceBuffer, varianceFactors[i]);

                // Also filter the second moment estimates
                filter.Apply(moments[i], momentBuffer);

                // Compute the ratio for all non-zero pixels
                for (int row = 0; row < height; ++row) {
                    for (int col = 0; col < width; ++col) {
                        var variance = varianceFactors[i][col, row].Value;
                        var moment = momentBuffer[col, row].Value;
                        if (variance > 0) {
                            varianceBuffer[col, row] = new(moment / variance);
                        } else {
                            varianceBuffer[col, row] = new(1);
                        }
                    }
                }

                // Apply a wide filter to the ratio image, too
                filter.Apply(varianceBuffer, varianceFactors[i]);
            }

            isReady = true;
        }

        public void Add(int cameraPathEdges, int lightPathEdges, int totalEdges,
                        Vector2 filmPoint, ColorRGB value) {
            bool isNextEvent = lightPathEdges == 0 && cameraPathEdges == totalEdges - 1;
            if (isNextEvent && cameraPathEdges > 1) { // Direct illumination has zero covariance
                moments[cameraPathEdges - 2].Splat(filmPoint.X, filmPoint.Y,
                    new((value * value / curIteration).Average));
                pixelValues[cameraPathEdges - 2].Splat(filmPoint.X, filmPoint.Y,
                    new(value.Average / curIteration));
            }
        }

        public float Get(int cameraPathEdges, Vector2 filmPoint) {
            if (!isReady) return 1.0f;
            if (cameraPathEdges < 2) return 1.0f;
            return varianceFactors[cameraPathEdges - 2][filmPoint.X, filmPoint.Y].Value;
        }

        public void WriteToFiles(string basename) {
            for (int i = 0; i < varianceFactors.Count; ++i) {
                var filename = $"{basename}-nextevt-{i+2}.exr";
                Image<Scalar>.WriteToFile(varianceFactors[i], filename);

                filename = $"{basename}-moments-nextevt-{i+2}.exr";
                Image<Scalar>.WriteToFile(moments[i], filename);

                filename = $"{basename}-means-nextevt-{i+2}.exr";
                Image<Scalar>.WriteToFile(pixelValues[i], filename);
            }
        }

        bool isReady = false;
        int curIteration = 0;
        int maxDepth;
        List<Image<Scalar>> moments;
        List<Image<Scalar>> pixelValues;
        List<Image<Scalar>> varianceFactors;
    }

    public class VarAwareMisBidir : ClassicBidir {
        NextEventVarianceFactors varianceFactors;

        public override void RegisterSample(ColorRGB weight, float misWeight, Vector2 pixel,
                                            int cameraPathLength, int lightPathLength, int fullLength) {
            base.RegisterSample(weight, misWeight, pixel, cameraPathLength, lightPathLength, fullLength);
            varianceFactors.Add(cameraPathLength, lightPathLength, fullLength, pixel, weight);
        }

        public override ColorRGB OnCameraHit(CameraPath path, RNG rng, int pixelIndex, Ray ray,
                                             SurfacePoint hit, float pdfFromAncestor, ColorRGB throughput,
                                             int depth, float toAncestorJacobian) {
            ColorRGB value = ColorRGB.Black;

            // Was a light hit?
            Emitter light = scene.QueryEmitter(hit);
            if (light != null) {
                value += throughput * OnEmitterHit(light, hit, ray, path, toAncestorJacobian);
            }

            // Perform connections if the maximum depth has not yet been reached
            if (depth < MaxDepth) {
                if (EnableConnections)
                    value += throughput * BidirConnections(pixelIndex, hit, -ray.Direction, rng, path, toAncestorJacobian);

                var nextEvtSum = ColorRGB.Black;
                var nextEvtSumSqr = ColorRGB.Black;
                for (int i = 0; i < NumShadowRays; ++i) {
                    var c = throughput * PerformNextEventEstimation(ray, hit, rng, path, toAncestorJacobian);
                    nextEvtSum += c;
                    nextEvtSumSqr += c * c;
                    value += c;
                }

            }

            return value;
        }

        public override void PostIteration(uint iteration) {
            varianceFactors.EndIteration();
        }

        public override void PreIteration(uint iteration) {
            varianceFactors.StartIteration();
        }

        public override void Render(Scene scene) {
            varianceFactors = new NextEventVarianceFactors(MaxDepth, scene.FrameBuffer.Width,
                scene.FrameBuffer.Height);

            base.Render(scene);

            string path = System.IO.Path.Join(scene.FrameBuffer.Basename, "variance-factors");
            varianceFactors.WriteToFiles(path);
        }

        public float ComputeNeeFactor(BidirPathPdfs pdfs, PathVertex? lightVertex, int numPdfs,
                                     int lastCameraVertexIdx, float pdfNextEvent, Vector2 pixel) {
            if (numPdfs <= 2) {
                return 1; // Direct illumination has no correlation
            }

            float factor = varianceFactors.Get(numPdfs - 2, pixel);

            // Retrieve the next event pdf from the second to last light path vertex (if it exists)
            while (lightVertex?.Depth > 2) {
                var ancestor = lightPaths.PathCache[lightVertex.Value.PathId, lightVertex.Value.AncestorId];
                lightVertex = ancestor;
            }
            if (lightVertex?.Depth == 2) {
                pdfNextEvent = lightVertex.Value.PdfNextEventAncestor;
            }

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

            float factor = ComputeNeeFactor(pathPdfs, null, numPdfs, lastCameraVertexIdx, pdfNextEvent, cameraPath.Pixel);
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

            ComputeNeeFactor(pathPdfs, lightVertex, numPdfs, lastCameraVertexIdx, pdfNextEvent, pixel);

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

            ComputeNeeFactor(pathPdfs, lightVertex, numPdfs, lastCameraVertexIdx, pdfNextEvent, cameraPath.Pixel);

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

            float factor = ComputeNeeFactor(pathPdfs, null, numPdfs, lastCameraVertexIdx, pdfNextEvent, cameraPath.Pixel);
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