﻿using System;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using UIKit;
using CFTimeInterval = System.Double;

namespace YSImagePicker.Views
{
    [Register("RecordButton")]
    public class RecordButton : StationaryButton
    {
        private float _outerBorderWidth = 3;
        private float _innerBorderWidth = 1.5f;
        private float _pressDepthFactor = 0.9f;
        private bool _highlighted;

        //TODO: CHeck how to applie setters
        public float OuterBorderWidth
        {
            get => _outerBorderWidth;
            set
            {
                _outerBorderWidth = value;
                SetNeedsUpdateCircleLayers();
            }
        }

        public float InnerBorderWidth
        {
            get => _innerBorderWidth;
            set
            {
                _innerBorderWidth = value;
                SetNeedsUpdateCircleLayers();
            }
        }

        public float PressDepthFactor
        {
            get => _pressDepthFactor;
            set
            {
                _pressDepthFactor = value;
                SetNeedsUpdateCircleLayers();
            }
        }

        public override bool Highlighted
        {
            get => _highlighted;
            set
            {
                if (Selected == false && value != Highlighted && value == true)
                {
                    UpdateCircleLayers(Views.State.Pressed, true);
                }

                _highlighted = value;
            }
        }

        private bool _needsUpdateCircleLayers = true;
        private CALayer _outerCircleLayer;
        private CALayer _innerCircleLayer;

        public float InnerCircleLayerInset => OuterBorderWidth + InnerBorderWidth;

        private State _layersState = Views.State.Initial;

        public RecordButton(IntPtr handler) : base(handler)
        {
            _outerCircleLayer = new CALayer();
            _innerCircleLayer = new CALayer();

            BackgroundColor = UIColor.Clear;
            Layer.AddSublayer(_outerCircleLayer);
            Layer.AddSublayer(_innerCircleLayer);
            CATransaction.DisableActions = true;

            _outerCircleLayer.BackgroundColor = UIColor.Clear.CGColor;
            _outerCircleLayer.CornerRadius = Bounds.Width / 2;
            _outerCircleLayer.BorderWidth = OuterBorderWidth;
            _outerCircleLayer.BorderColor = TintColor.CGColor;

            _innerCircleLayer.BackgroundColor = UIColor.Red.CGColor;

            CATransaction.Commit();
        }

        public override void SelectionDidChange(bool animated)
        {
            base.SelectionDidChange(animated);
            UpdateCircleLayers(Selected ? Views.State.Recording : Views.State.Initial, animated);
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            if (_needsUpdateCircleLayers)
            {
                CATransaction.DisableActions = true;
                _outerCircleLayer.Frame = Bounds;
                _innerCircleLayer.Frame = Bounds.Inset(InnerCircleLayerInset, InnerCircleLayerInset);
                _innerCircleLayer.CornerRadius = Bounds.Inset(InnerCircleLayerInset, InnerCircleLayerInset).Width / 2;
                _needsUpdateCircleLayers = false;
                CATransaction.Commit();
            }
        }

        private void SetNeedsUpdateCircleLayers()
        {
            _needsUpdateCircleLayers = true;
            SetNeedsLayout();
        }

        public void UpdateCircleLayers(State state, bool animated)
        {
            if (_layersState == state)
            {
                return;
            }

            _layersState = state;

            switch (_layersState)
            {
                case Views.State.Initial:
                    SetInnerLayer(false, animated);
                    break;
                case Views.State.Pressed:
                    SetInnerLayerPressed(animated);
                    break;
                case Views.State.Recording:
                    SetInnerLayer(true, animated);
                    break;
            }
        }

        private void SetInnerLayerPressed(bool animated)
        {
            if (animated)
            {
                _innerCircleLayer.AddAnimation(TransformAnimation(_pressDepthFactor, 0.25), null);
            }
            else
            {
                CATransaction.DisableActions = true;
                _innerCircleLayer.SetValueForKeyPath(FromObject(_pressDepthFactor), new NSString("transform.scale"));
                CATransaction.Commit();
            }
        }

        private void SetInnerLayer(bool recording, bool animated)
        {
            if (recording)
            {
                _innerCircleLayer.AddAnimation(TransformAnimation(0.5f, 0.15), null);
                _innerCircleLayer.CornerRadius = 8;
            }
            else
            {
                _innerCircleLayer.AddAnimation(TransformAnimation(1, 0.25), null);
                _innerCircleLayer.CornerRadius =
                    Bounds.Inset(InnerCircleLayerInset, InnerCircleLayerInset).Width / 2;
            }
        }

        private CAAnimation TransformAnimation(float value, CFTimeInterval duration)
        {
            var animation = new CABasicAnimation
            {
                KeyPath = "transform.scale",
                From = _innerCircleLayer.PresentationLayer.ValueForKeyPath(new NSString("transform.scale")),
                To = FromObject(value),
                Duration = duration,
                TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.EaseInEaseOut),
                BeginTime = CAAnimation.CurrentMediaTime(),
                FillMode = CAFillMode.Forwards,
                RemovedOnCompletion = false
            };
            return animation;
        }
    }

    public enum State
    {
        Initial,
        Pressed,
        Recording
    }
}