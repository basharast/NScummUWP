using System;
namespace NScumm.GCServices
{
    public class GCService
    {
        public const uint MaxSamplesQueueSize = 44100 * 4;
        public const uint GCReserveSize = MaxSamplesQueueSize;
        public bool EnableGCPrevent = true;

        public  void TryStartNoGCRegionCall()
        {
            try
            {
                    if (EnableGCPrevent)
                    {
                        GC.TryStartNoGCRegion(GCReserveSize, true);
                    }
            }
            catch (Exception ea)
            {

            }
        }

        public  void EndNoGCRegionCall()
        {
            try
            {
                if (EnableGCPrevent)
                {
                    GC.EndNoGCRegion();
                }
            }
            catch (Exception es)
            {
            }
        }

    }

}
