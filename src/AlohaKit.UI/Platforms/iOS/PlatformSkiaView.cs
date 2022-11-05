﻿using AlohaKit.UI.Extensions;
using Foundation;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Platform;
using UIKit;

namespace AlohaKit.UI
{
	public class PlatformSkiaView : Microsoft.Maui.Graphics.Skia.Views.SkiaGraphicsView
	{
		ISkiaGraphicsView _graphicsView;
		UIHoverGestureRecognizer _hoverGesture;
		RectF _rect;
		bool _pressedContained = false;

		public PlatformSkiaView(IDrawable drawable = null) : base(drawable)
		{

		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
			_rect = Bounds.AsRectangleF();
		}

		public void Connect(ISkiaGraphicsView graphicsView)
		{
			_graphicsView = graphicsView;

			if (OperatingSystem.IsIOSVersionAtLeast(13))
				AddGestureRecognizer(_hoverGesture = new UIHoverGestureRecognizer(OnHover));
		}

		public void Disconnect()
		{
			RemoveGestureRecognizer(_hoverGesture!);
			_hoverGesture = null;
			_graphicsView = null;
		}

		void OnHover()
		{
			if (_hoverGesture!.State == UIGestureRecognizerState.Began)
			{
				var touch = _hoverGesture.LocationInView(this);
				_graphicsView?.StartHoverInteraction(new[] { (PointF)touch.ToPoint() });
			}
			else if (_hoverGesture.State == UIGestureRecognizerState.Changed)
			{
				var touch = _hoverGesture.LocationInView(this);
				_graphicsView?.MoveHoverInteraction(new[] { (PointF)touch.ToPoint() });
			}
			else
				_graphicsView?.EndHoverInteraction();
		}

		public override void TouchesBegan(NSSet touches, UIEvent evt)
		{
			if (!IsFirstResponder)
				BecomeFirstResponder();
			var viewPoints = this.GetPointsInView(evt);
			_graphicsView?.StartInteraction(viewPoints);
			_pressedContained = true;
		}

		public override void TouchesMoved(NSSet touches, UIEvent evt)
		{
			var viewPoints = this.GetPointsInView(evt);
			_pressedContained = _rect.ContainsAny(viewPoints);
			_graphicsView?.DragInteraction(viewPoints);
		}

		public override void TouchesEnded(NSSet touches, UIEvent evt) =>
			_graphicsView?.EndInteraction(this.GetPointsInView(evt), _pressedContained);

		public override void TouchesCancelled(NSSet touches, UIEvent evt)
		{
			_pressedContained = false;
			_graphicsView?.CancelInteraction();
		}
	}
}