using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using SeeSharp.Image;
using SimpleImageIO;

namespace MisForCorrelatedBidir.Common {
    public class MergeVarianceFactors {
        public MergeVarianceFactors(int maxDepth, int width, int height, int numPaths) {
            this.maxDepth = maxDepth;
            this.numPaths = numPaths;

            moments = new(maxDepth - 2);
            pixelValues = new(maxDepth - 2);
            varianceFactors = new(maxDepth - 2);
            for (int len = 3; len <= maxDepth; ++len) { // all depths with correlated merges (i.e., no DI)
                moments.Add(new(len - 2));
                pixelValues.Add(new(len - 2));
                varianceFactors.Add(new(len - 2));
                for (int i = 2; i < len; ++i) { // all merges with correlation for paths of length "len"
                    moments[^1].Add(new(width, height));
                    pixelValues[^1].Add(new(width, height));
                    varianceFactors[^1].Add(new(width, height));
                }
            }
        }

        public void StartIteration() {
            curIteration++;

            // Scale values of the previous iteration to account for having more samples
            if (curIteration > 1) {
                Parallel.For(0, moments.Count, i => {
                    for (int k = 0; k < moments[i].Count; ++k) {
                        moments[i][k].Scale((curIteration - 1.0f) / curIteration);
                        pixelValues[i][k].Scale((curIteration - 1.0f) / curIteration);
                    }
                });
            }
        }

        public void EndIteration() {
            int width = moments[0][0].Width;
            int height = moments[0][0].Height;
            var filter = new BoxFilter(4);
            MonochromeImage momentBuffer = new(width, height);
            MonochromeImage varianceBuffer = new(width, height);

            // Compute the variance factors for use in the next iteration
            for (int i = 0; i < moments.Count; ++i) {
                for (int k = 0; k < moments[i].Count; ++k) {
                    // Estimate the pixel variances:
                    // First, we blur the image. Then, we subtract the blurred version from the original.
                    // Finally, we compute and square the difference, multiplying by the number of iterations
                    // to obtain a coarse estimate of the variance in a single iteration.
                    filter.Apply(pixelValues[i][k], varianceBuffer);
                    Parallel.For(0, height, row => {
                        for (int col = 0; col < width; ++col) {
                            var value = pixelValues[i][k].GetPixel(col, row);
                            var delta = value - varianceBuffer.GetPixel(col, row);
                            var variance = delta * delta * curIteration;
                            varianceBuffer.SetPixel(col, row, variance);
                        }
                    });
                    filter.Apply(varianceBuffer, varianceFactors[i][k]);

                    // Also filter the second moment estimates
                    filter.Apply(moments[i][k], momentBuffer);

                    // Compute the ratio for all non-zero pixels
                    Parallel.For(0, height, row => {
                        for (int col = 0; col < width; ++col) {
                            var variance = varianceFactors[i][k].GetPixel(col, row);
                            var moment = momentBuffer.GetPixel(col, row);
                            if (variance > 0 && moment > 0) {
                                varianceBuffer.SetPixel(col, row, moment / variance);
                            } else {
                                varianceBuffer.SetPixel(col, row, 1);
                            }
                        }
                    });

                    // Apply a wide filter to the ratio image, too
                    filter.Apply(varianceBuffer, varianceFactors[i][k]);
                }
            }

            isReady = true;
        }

        public void Add(int cameraPathEdges, int lightPathEdges, int totalEdges,
                        Vector2 filmPoint, RgbColor value) {
            bool isMerge = lightPathEdges > 0 && lightPathEdges + cameraPathEdges == totalEdges;
            if (isMerge && cameraPathEdges > 1) { // Primary merges have zero covariance
                float v = value.Average;
                moments[totalEdges - 3][cameraPathEdges - 2].
                    AtomicAdd((int)filmPoint.X, (int)filmPoint.Y, v * v / curIteration);
                pixelValues[totalEdges - 3][cameraPathEdges - 2].
                    AtomicAdd((int)filmPoint.X, (int)filmPoint.Y, v / curIteration);
            }
        }

        public float Get(int cameraPathEdges, int totalEdges, Vector2 filmPoint) {
            if (!isReady) return 1.0f;
            if (cameraPathEdges < 2) return 1.0f;
            return varianceFactors[totalEdges - 3][cameraPathEdges - 2]
                .GetPixel((int)filmPoint.X, (int)filmPoint.Y);
        }

        public void WriteToFiles(string basename) {
            for (int i = 0; i < varianceFactors.Count; ++i) {
                for (int k = 0; k < varianceFactors[i].Count; ++k) {
                    var filename = $"{basename}-depth-{i+3}-merge-{k+2}.exr";
                    varianceFactors[i][k].WriteToFile(filename);
                }
            }
        }

        bool isReady = false;
        int curIteration = 0;
        int maxDepth;
        int numPaths;
        List<List<MonochromeImage>> moments;
        List<List<MonochromeImage>> pixelValues;
        List<List<MonochromeImage>> varianceFactors;
    }
}