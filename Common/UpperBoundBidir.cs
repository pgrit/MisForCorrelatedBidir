using System.Numerics;
using SeeSharp.Core.Geometry;
using SeeSharp.Integrators.Common;

namespace MisForCorrelatedBidir.Common {
    public class UpperBoundBidir : SeeSharp.Integrators.Bidir.ClassicBidir {
        public override float NextEventPdf(SurfacePoint from, SurfacePoint to) =>
            // Apply the upper bound approach (set the number of samples to one by division)
            base.NextEventPdf(from, to) / NumShadowRays;

        public override float EmitterHitMis(CameraPath cameraPath, float pdfEmit, float pdfNextEvent) {
            if (cameraPath.Vertices.Count == 2) {
                // use the actual number of samples for DI (no correlation)
                return base.EmitterHitMis(cameraPath, pdfEmit, pdfNextEvent * NumShadowRays);
            } else {
                return base.EmitterHitMis(cameraPath, pdfEmit, pdfNextEvent);
            }
        }

        public override float LightTracerMis(PathVertex lightVertex, float pdfCamToPrimary, float pdfReverse,
                                             float pdfNextEvent, Vector2 pixel, float distToCam)
        // pdfNextEvent is either zero, or this is a DI path and we should not ignore the number of shadow rays
        => base.LightTracerMis(lightVertex, pdfCamToPrimary, pdfReverse, pdfNextEvent * NumShadowRays, pixel, distToCam);

        public override float NextEventMis(CameraPath cameraPath, float pdfEmit, float pdfNextEvent,
                                           float pdfHit, float pdfReverse) {
            if (cameraPath.Vertices.Count == 1) {
                // This is a DI path, there is no correlation
                return base.NextEventMis(cameraPath, pdfEmit, pdfNextEvent, pdfHit, pdfReverse);
            } else {
                // Apply the upper bound approach (set the number of samples to one by division)
                return base.NextEventMis(cameraPath, pdfEmit, pdfNextEvent / NumShadowRays, pdfHit, pdfReverse);
            }
        }
    }
}