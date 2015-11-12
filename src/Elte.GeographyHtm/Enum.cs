using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elte.GeographyHtm
{
    [Flags]
    public enum Markup
    {
        Undefined = 0,

        Inner = 1,
        Partial = 2,
        Reject = 4,
    }

    [Flags]
    public enum CoverBuilderStopCondition : int
    {
        MaxRangesReached = 1,
        MaxLevelReached = 2,
        MaxTimeReached = 4,
        AreaFractionReached = 8,

        Default = MaxTimeReached | AreaFractionReached,
    }
}
