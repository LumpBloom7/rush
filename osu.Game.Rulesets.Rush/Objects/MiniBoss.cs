// Copyright (c) Shane Woolcock. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Rush.Judgements;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Rush.Objects
{
    public class MiniBoss : RushHitObject, IHasEndTime
    {
        public static readonly int DEFAULT_REQUIRED_HITS = 5;

        public double EndTime
        {
            get => StartTime + Duration;
            set => Duration = value - StartTime;
        }

        public double Duration { get; set; }

        public int RequiredHits = DEFAULT_REQUIRED_HITS;

        protected override void CreateNestedHitObjects()
        {
            base.CreateNestedHitObjects();

            for (int i = 0; i < RequiredHits; i++)
                AddNested(new MiniBossTick());
        }

        public override Judgement CreateJudgement() => new RushMiniBossJudgement();

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;
    }
}
