/*
* The MIT License (MIT)
*
* Copyright (c) 2014 Robin Chutaux
* Copyright (c) 2014 Tomasz Cielecki
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/

using System;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;

namespace Cheesebaron.RippleEffect
{
    public class RippleView
        : RelativeLayout
    {
		public event EventHandler OnCompletition;

        private int _width;
        private int _height;
        private int _frameRate = 10;
        private int _duration = 400;
        private int _paintAlpha = 90;
        private Handler _canvasHandler;
        private float _radiusMax;
        private bool _animationRunning;
        private int _timer;
        private int _timerEmpty;
        private int _durationEmpty = -1;
        private float _x = -1;
        private float _y = -1;
		private int _zoomDuration;
		private float _zoomScale;
        private Animation _scaleAnimation;
        private bool _hasToZoom;
        private bool _isCentered;
        private int _rippleType;
        private Paint _paint;
        private Bitmap _originBitmap;
        private Color _rippleColor;
        //private View _childView;
        private int _ripplePadding;
        private GestureDetector _gestureDetector;

        protected RippleView(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer) { }

        public RippleView(Context context)
            : base(context) { }

        public RippleView(Context context, IAttributeSet attrs)
            : this(context, attrs, 0) { }

        public RippleView(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
        {
            Init(context, attrs);
        }

        private void Init(Context context, IAttributeSet attrs)
        {
			if (IsInEditMode)
				return;

            var a = context.ObtainStyledAttributes(attrs, Resource.Styleable.RippleView);
            _rippleColor = a.GetColor(Resource.Styleable.RippleView_rv_color,
				Resources.GetColor(Resource.Color.__rippleViewDefaultColor));
            _rippleType = a.GetInt(Resource.Styleable.RippleView_rv_type, 0);
            _hasToZoom = a.GetBoolean(Resource.Styleable.RippleView_rv_zoom, false);
            _isCentered = a.GetBoolean(Resource.Styleable.RippleView_rv_centered, false);
            _duration = a.GetInt(Resource.Styleable.RippleView_rv_rippleDuration, _duration);
            _frameRate = a.GetInt(Resource.Styleable.RippleView_rv_framerate, _frameRate);
            _paintAlpha = a.GetInt(Resource.Styleable.RippleView_rv_alpha, _paintAlpha);
            _ripplePadding = a.GetDimensionPixelSize(Resource.Styleable.RippleView_rv_ripplePadding, 0);
            _canvasHandler = new Handler();
			_zoomScale = a.GetFloat(Resource.Styleable.RippleView_rv_zoomScale, 1.03f);
			_zoomDuration = a.GetInt(Resource.Styleable.RippleView_rv_zoomDuration, 200);
            _scaleAnimation = AnimationUtils.LoadAnimation(context, Resource.Animation.zoom);
            _scaleAnimation.Duration = a.GetInt(Resource.Styleable.RippleView_rv_zoomDuration, 150);
            _paint = new Paint(PaintFlags.AntiAlias);
            _paint.SetStyle(Paint.Style.Fill);
            _paint.Color = _rippleColor;
            _paint.Alpha = _paintAlpha;

            a.Recycle();

            _gestureDetector = new GestureDetector(context, new RippleGestureDetector(this));

            SetWillNotDraw(false);
            DrawingCacheEnabled = true;
        }

        /*public override void AddView(View child, int index, ViewGroup.LayoutParams @params)
        {
            _childView = child;
            base.AddView(child, index, @params);
        }*/

        public override void Draw(Canvas canvas)
        {
            base.Draw(canvas);
            if (!_animationRunning) return;

            if (_duration <= _timer * _frameRate)
            {
                _animationRunning = false;
                _timer = 0;
                _durationEmpty = -1;
                _timerEmpty = 0;
                canvas.Restore();
                Invalidate();
				if (OnCompletition != null)
					OnCompletition (this, new EventArgs ());
                return;
            }

            _canvasHandler.PostDelayed(Invalidate, _frameRate);

            if (_timer == 0)
                canvas.Save();

            canvas.DrawCircle(_x, _y, (_radiusMax * (((float) _timer * _frameRate) / _duration)), _paint);

            _paint.Color = Resources.GetColor(Android.Resource.Color.HoloRedLight);

            if (_rippleType == 1 && _originBitmap != null && (((float) _timer * _frameRate) / _duration) > 0.4f)
            {
                if (_durationEmpty == -1)
                    _durationEmpty = _duration - _timer * _frameRate;

                _timerEmpty++;
                using (var tmpBitmap = GetCircleBitmap((int) (_paintAlpha - ((_paintAlpha) *
                        (((float) _timerEmpty * _frameRate) / (_durationEmpty))))))
                {
                    canvas.DrawBitmap(tmpBitmap, 0, 0, _paint);
                    tmpBitmap.Recycle();
                }
            }

            _paint.Color = _rippleColor;

            if (_rippleType == 1)
            {
                if ((((float) _timer * _frameRate) / _duration) > 0.6f)
                    _paint.Alpha =
                        (int) (_paintAlpha - (_paintAlpha * (((float) _timerEmpty * _frameRate) / _durationEmpty)));
                else
                    _paint.Alpha = _paintAlpha;
            }
            else
                _paint.Alpha = (int)(_paintAlpha - (_paintAlpha * (((float)_timer * _frameRate) / _duration)));

			_timer++;
        }

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);
            _width = w;
            _height = h;

			_scaleAnimation = new ScaleAnimation(1.0f, _zoomScale, 1.0f, _zoomScale, w / 2, h / 2);
			_scaleAnimation.Duration = _zoomDuration;
			_scaleAnimation.RepeatMode = RepeatMode.Reverse;
			_scaleAnimation.RepeatCount = 1;
        }

		public void AnimateRipple(MotionEvent ev) {
			CreateAnimation(ev.GetX(), ev.GetY());
		}

		public void animateRipple(float x, float y) {
			CreateAnimation(x, y);
		}

		private void CreateAnimation(float x, float y)
		{
			if (!Enabled || _animationRunning) return;

			if (_hasToZoom)
				StartAnimation(_scaleAnimation);

			_radiusMax = Math.Max(_width, _height);

			if (_rippleType != 2)
				_radiusMax /= 2;

			_radiusMax -= _ripplePadding;

			if (_isCentered || _rippleType == 1) {
				_x = MeasuredHeight / 2;
				_y = MeasuredWidth / 2;
			} else {
				_x = x;
				_y = y;
			}

			_animationRunning = true;

			if (_rippleType == 1 && _originBitmap == null)
				_originBitmap = GetDrawingCache(true);

			Invalidate();
		}

        public override bool OnTouchEvent(MotionEvent ev)
        {
            if (_gestureDetector.OnTouchEvent(ev))
            {
				AnimateRipple (ev);
				SendClickEvent (false);
            }

            return true;
        }

        public override bool OnInterceptTouchEvent(MotionEvent ev)
        {
			OnTouchEvent (ev);
			return false;
        }

		private void SendClickEvent(bool isLongClick)
		{
			if (Parent is AdapterView) {
				AdapterView adapterView = (AdapterView) Parent;
				int position = adapterView.GetPositionForView(this);
				long id = adapterView.GetItemIdAtPosition(position);
				if (isLongClick) {
					if (adapterView.OnItemLongClickListener != null)
						adapterView.OnItemLongClickListener.OnItemLongClick(adapterView, this, position, id);
				} else {
					if (adapterView.OnItemClickListener != null)
						adapterView.OnItemClickListener.OnItemClick(adapterView, this, position, id);
				}
			}
		}

        private Bitmap GetCircleBitmap(int radius)
        {
            var output = Bitmap.CreateBitmap(_originBitmap.Width, _originBitmap.Height, Bitmap.Config.Argb8888);
            using (var canvas = new Canvas(output))
            using (var paint = new Paint(PaintFlags.AntiAlias))
            using (var rect = new Rect((int)(_x - radius), (int)(_y - radius), (int)(_x + radius), (int)(_y + radius)))
            {
                canvas.DrawARGB(0, 0, 0, 0);
                canvas.DrawCircle(_x, _y, radius, paint);

                paint.SetXfermode(new PorterDuffXfermode(PorterDuff.Mode.SrcIn));
                canvas.DrawBitmap(_originBitmap, rect, rect, paint);

                return output;
            }
        }

        private class RippleGestureDetector : GestureDetector.SimpleOnGestureListener
        {
			RippleView rippleView;
			public RippleGestureDetector(RippleView rippleView) {
				this.rippleView = rippleView;
			}

			public override void OnLongPress (MotionEvent e)
			{
				base.OnLongPress (e);
				rippleView.AnimateRipple (e);
				rippleView.SendClickEvent (true);
			}

            public override bool OnSingleTapConfirmed(MotionEvent e)
            {
                return true;
            }

            public override bool OnSingleTapUp(MotionEvent e)
            {
                return true;
            }
        }
    }
}