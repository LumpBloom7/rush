// Copyright (c) Shane Woolcock. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK;

namespace osu.Game.Rulesets.Rush.Objects.Drawables
{
    public class DrawableDualOrb : DrawableRushHitObject<DualOrb>
    {
        private readonly Container<DrawableOrb> airOrbContainer;
        private readonly Container<DrawableOrb> groundOrbContainer;
        private readonly Box joinBox;

        public DrawableOrb Air => airOrbContainer.Child;
        public DrawableOrb Ground => groundOrbContainer.Child;

        public DrawableDualOrb(DualOrb hitObject)
            : base(hitObject)
        {
            RelativeSizeAxes = Axes.Y;
            Height = 1f;

            Content.AddRange(new Drawable[]
            {
                joinBox = new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Y,
                    Size = new Vector2(10f, 1f)
                },
                airOrbContainer = new Container<DrawableOrb> { RelativeSizeAxes = Axes.Both },
                groundOrbContainer = new Container<DrawableOrb> { RelativeSizeAxes = Axes.Both },
            });
        }

        protected override void AddNestedHitObject(DrawableHitObject hitObject)
        {
            base.AddNestedHitObject(hitObject);

            switch (hitObject)
            {
                case DrawableOrb orb:
                    (orb.HitObject.Lane == LanedHitLane.Air ? airOrbContainer : groundOrbContainer).Add(orb);
                    break;
            }
        }

        protected override void ClearNestedHitObjects()
        {
            base.ClearNestedHitObjects();
            airOrbContainer.Clear();
            groundOrbContainer.Clear();
        }

        protected override DrawableHitObject CreateNestedHitObject(HitObject hitObject)
        {
            switch (hitObject)
            {
                case Orb orb:
                    return new DrawableOrb(orb);
            }

            return base.CreateNestedHitObject(hitObject);
        }

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();

            joinBox.Colour = ColourInfo.GradientVertical(AIR_ACCENT_COLOUR, GROUND_ACCENT_COLOUR);
            joinBox.Show();
        }

        protected override void OnDirectionChanged(ValueChangedEvent<ScrollingDirection> e)
        {
            base.OnDirectionChanged(e);

            Origin = e.NewValue == ScrollingDirection.Left ? Anchor.CentreLeft : Anchor.CentreRight;
        }

        public override bool OnPressed(RushAction action)
        {
            if (AllJudged)
                return false;

            if (!Air.AllJudged && Air.LaneMatchesAction(action))
                Air.UpdateResult();
            else if (!Ground.AllJudged && Ground.LaneMatchesAction(action))
                Ground.UpdateResult();
            else
                return false;

            if (Air.AllJudged && Ground.AllJudged)
                UpdateResult(true);

            return true;
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (AllJudged)
                return;

            if (!Air.AllJudged || !Ground.AllJudged)
                return;

            // If we missed both air and ground, it's an overall miss.
            // If we missed only one of them, it's an overall Meh.
            // If we hit both, the overall judgement is the lowest score of the two.
            ApplyResult(r =>
            {
                var lowestResult = Air.Result.Type < Ground.Result.Type ? Air.Result.Type : Ground.Result.Type;

                if (Air.IsHit != Ground.IsHit)
                    r.Type = HitResult.Meh;
                else if (!Air.IsHit && !Ground.IsHit)
                    r.Type = HitResult.Miss;
                else
                    r.Type = lowestResult;
            });
        }

        protected override void UpdateStateTransforms(ArmedState state)
        {
            const float animation_time = 300f;

            switch (state)
            {
                case ArmedState.Idle:
                    UnproxyContent();
                    break;

                case ArmedState.Miss:
                    this.FadeOut(animation_time);
                    break;

                case ArmedState.Hit:
                    ProxyContent();
                    joinBox.Hide();
                    this.FadeOut(animation_time);
                    break;
            }
        }
    }
}
