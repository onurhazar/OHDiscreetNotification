//  OHDiscreetNotificationView.cs
//
//  Created by Onur Hazar on 06-02-23.
//  Copyright 2023 Onur Hazar. All rights reserved.
//

using System;
using UIKit;
using static CoreFoundation.DispatchQueue;

namespace OHDiscreetNotification
{
    public class OHDiscreetNotificationView : UIView
    {
        const float OHDiscreetNotificationViewBorderSize = 25;
        const float OHDiscreetNotificationViewPadding = 5;
        const float OHDiscreetNotificationViewHeight = 30;
        const string OHShowAnimation = "show";
        const string OHHideAnimation = "hide";
        const string OHChangeProperty = "changeProperty";
        const string OHDiscreetNotificationViewTextKey = "text";
        const string OHDiscreetNotificationViewActivityKey = "activity";

        UIActivityIndicatorView? activityIndicator;
        bool listenTap = false;
        bool animating = false;
        NSDictionary? animationDict;

        //You can access the label and the activity indicator to change its values. 
        //If you want to change the text or the activity itself, use textLabel and showActivity properties.
        UILabel? label;
        public UILabel? Label
        {
            get
            {
                if (label == null)
                {
                    label = new UILabel()
                    {
                        Font = UIFont.SystemFontOfSize(15f),
                        TextColor = UIColor.White,
                        ShadowColor = UIColor.Black,
                        ShadowOffset = new CGSize(width: 0, height: 1),
                        BaselineAdjustment = UIBaselineAdjustment.AlignCenters,
                        BackgroundColor = UIColor.Clear
                    };

                    this.AddSubview(label);
                }

                return label;
            }

            set
            {
                label = value;
            }
        }

        //The content view where the notification will be shown
        public UIView? View
        {
            get
            {
                return Superview;
            }

            set
            {
                if (this.View == null || this.View != value)
                {
                    RemoveFromSuperview();
                    value?.AddSubview(this);
                    BringSubviewToFront(this);
                    SetNeedsLayout();
                }
            }
        }

        OHDNPresentationMode presentationMode;
        public OHDNPresentationMode PresentationMode
        {
            get
            {
                return presentationMode;
            }

            set
            {
                if (presentationMode != value)
                {
                    var showing = this.Showing;

                    presentationMode = value;

                    if (presentationMode == OHDNPresentationMode.Top)
                    {
                        AutoresizingMask = UIViewAutoresizing.FlexibleRightMargin | UIViewAutoresizing.FlexibleLeftMargin | UIViewAutoresizing.FlexibleBottomMargin;
                    }
                    else if (presentationMode == OHDNPresentationMode.Bottom)
                    {
                        AutoresizingMask = UIViewAutoresizing.FlexibleRightMargin | UIViewAutoresizing.FlexibleLeftMargin | UIViewAutoresizing.FlexibleTopMargin;
                    }

                    Center = showing ? showingCenter : hidingCenter;

                    SetNeedsDisplay();
                    PlaceOnGrid();
                }
            }
        }

        CGPoint showingCenter
        {
            get
            {
                float y = 0;
                if (presentationMode == OHDNPresentationMode.Top)
                {
                    y = 15;
                }
                else if (presentationMode == OHDNPresentationMode.Bottom)
                {
                    y = (float)((this.View != null ? this.View.Frame.Size.Height : 0.0) - 15.0);
                }
                return new CGPoint(x: (this.View != null ? this.View.Frame.Size.Width : 0.0) / 2, y: y);
            }
        }

        CGPoint hidingCenter
        {
            get
            {
                float y = 0;
                if (presentationMode == OHDNPresentationMode.Top)
                {
                    y = -15;
                }
                else if (presentationMode == OHDNPresentationMode.Bottom)
                {
                    y = (float)(15 + (this.View != null ? this.View.Frame.Size.Height : 0.0));
                }
                return new CGPoint(x: (this.View != null ? this.View.Frame.Size.Width : 0.0) / 2, y: y);
            }
        }

        public bool Showing
        {
            get
            {
                return Center.Y == showingCenter.Y;
            }
        }

        string? textLabel;
        public string? TextLabel
        {
            get
            {
                return label?.Text;
            }

            set
            {
                textLabel = value;

                if (label != null)
                    label.Text = value;

                if (Label != null)
                    Label.Text = value;

                SetNeedsLayout();
            }
        }

        bool showActivity;
        public bool ShowActivity
        {
            get
            {
                return activityIndicator != null;
            }

            set
            {
                if (value != this.showActivity)
                {
                    if (value)
                    {
                        activityIndicator = new UIActivityIndicatorView(style: UIActivityIndicatorViewStyle.White);
                        if (activityIndicator != null)
                            this.AddSubview(activityIndicator);
                    }
                    else
                    {
                        activityIndicator?.RemoveFromSuperview();
                        activityIndicator = null;
                    }

                    SetNeedsLayout();
                }
                showActivity = value;
            }
        }

        public OHDiscreetNotificationView(string? text, bool activity, OHDNPresentationMode presentationMode, UIView? view)
        {
            View = view;
            TextLabel = text;
            ShowActivity = activity;
            PresentationMode = presentationMode;

            Center = hidingCenter;
            SetNeedsLayout();

            UserInteractionEnabled = false;
            Opaque = false;
            BackgroundColor = UIColor.Clear;

            animating = false;
        }

        #region Drawing & Layout

        public override void LayoutSubviews()
        {
            var withActivity = activityIndicator != null;
            var baseWidth = (2 * OHDiscreetNotificationViewBorderSize) + ((withActivity ? 1 : 0) * OHDiscreetNotificationViewPadding);

            var maxLabelWidth = (this.View != null ? this.View.Frame.Size.Width : 0.0) -
                (this.activityIndicator != null ? this.activityIndicator.Frame.Size.Width : 0.0) * ((withActivity ? 1 : 0) - baseWidth);
            var maxLabelSize = new CGSize(width: maxLabelWidth, height: OHDiscreetNotificationViewHeight);

            float textSizeWidth = 0;

            if (!string.IsNullOrEmpty(textLabel))
            {
                var attributes = new UIStringAttributes();
                var paragraphStyle = new NSMutableParagraphStyle();

                if (paragraphStyle != null && label != null)
                {
                    paragraphStyle.LineBreakMode = UILineBreakMode.WordWrap;
                    attributes.ParagraphStyle = paragraphStyle;
                    attributes.Font = label.Font;
                }

                var textLabelStr = string.IsNullOrEmpty(textLabel) ? new NSString("") : new NSString(textLabel);
                var boundingRect = textLabelStr?.GetBoundingRect(size: maxLabelSize,
                    options: NSStringDrawingOptions.UsesLineFragmentOrigin | NSStringDrawingOptions.UsesFontLeading,
                    attributes: attributes, context: null);

                textSizeWidth = (float)boundingRect.GetValueOrDefault().Size.Width;
            }

            var activityIndicatorWidth = activityIndicator != null ? activityIndicator.Frame.Size.Width : 0.0;
            var bounds = new CGRect(x: 0, y: 0, width: baseWidth + textSizeWidth + activityIndicatorWidth, height: OHDiscreetNotificationViewHeight);

            if (!this.Bounds.Equals(bounds))
            {
                this.Bounds = bounds;
                SetNeedsDisplay();
            }

            if (activityIndicator == null)
            {
                if (label != null)
                    label.Frame = new CGRect(x: OHDiscreetNotificationViewBorderSize, y: 0, width: textSizeWidth, height: OHDiscreetNotificationViewHeight);
            }
            else
            {
                activityIndicator.Frame = new CGRect(x: OHDiscreetNotificationViewBorderSize,
                    y: OHDiscreetNotificationViewPadding,
                    width: activityIndicator != null ? activityIndicator.Frame.Size.Width : 0.0,
                    height: OHDiscreetNotificationViewHeight);
            }

            PlaceOnGrid();
        }

        public override void Draw(CGRect rect)
        {
            var myFrame = this.Bounds;
            float maxY = 0;
            float minY = 0;

            if (presentationMode == OHDNPresentationMode.Top)
            {
                maxY = (float)(myFrame.GetMinY() - 1);
                minY = (float)(myFrame.GetMaxY());
            }
            else if (presentationMode == OHDNPresentationMode.Bottom)
            {
                maxY = (float)(myFrame.GetMaxY() + 1);
                minY = (float)(myFrame.GetMinY());
            }

            var context = UIGraphics.GetCurrentContext();
            var path = new CGPath();
            path.MoveToPoint(transform: CGAffineTransform.MakeIdentity(), point: new CGPoint(x: myFrame.GetMinX(), y: maxY));

            path.AddCurveToPoint(transform: CGAffineTransform.MakeIdentity(),
                cp1: new CGPoint(x: myFrame.GetMinX() + OHDiscreetNotificationViewBorderSize, y: maxY),
                cp2: new CGPoint(x: myFrame.GetMinX(), y: minY),
                point: new CGPoint(x: myFrame.GetMinX() + OHDiscreetNotificationViewBorderSize, y: minY));

            path.AddLineToPoint(transform: CGAffineTransform.MakeIdentity(), point: new CGPoint(x: myFrame.GetMaxX() - OHDiscreetNotificationViewBorderSize, y: minY));

            path.AddCurveToPoint(transform: CGAffineTransform.MakeIdentity(),
                cp1: new CGPoint(x: myFrame.GetMaxX(), y: minY),
                cp2: new CGPoint(x: myFrame.GetMaxX() - OHDiscreetNotificationViewBorderSize, y: maxY),
                point: new CGPoint(x: myFrame.GetMaxX(), y: maxY));

            path.CloseSubpath();

            context?.SetFillColor(UIColor.Black.ColorWithAlpha(0.8f).CGColor);
            context?.SetStrokeColor(UIColor.Black.CGColor);

            context?.AddPath(path);
            context?.StrokePath();

            context?.AddPath(path);
            context?.FillPath();
        }

        #endregion

        #region Show & Hide

        public void ShowAnimated()
        {
            Show(true);
        }

        public void HideAnimated()
        {
            Hide(true);
        }

        public void HideAnimated(double timeInverval)
        {
            PerformSelector(new ObjCRuntime.Selector("HideAnimated"), withObject: null, delay: timeInverval);
        }

        public void ShowAndDismissAutomaticallyAnimated()
        {
            ShowAndDismiss(timeInterval: 1.0);
        }

        public void ShowAndDismiss(double timeInterval)
        {
            ShowAnimated();
            HideAnimated(timeInterval);
        }

        public void Show(bool animated)
        {
            Show(animated, name: OHShowAnimation);
        }

        public void Hide(bool animated)
        {
            Hide(animated, name: OHHideAnimation);
        }

        private void Show(bool animated, string name)
        {
            NSObject.CancelPreviousPerformRequest(aTarget: this, selector: new ObjCRuntime.Selector("HideAnimated"), argument: null);

            ShowOrHide(false, animated: animated, name: name);
        }

        private void Hide(bool animated, string name)
        {
            ShowOrHide(false, animated: animated, name: name);
        }

        private void ShowOrHide(bool hide, bool animated, string name)
        {
            if ((hide && Showing) || (!hide && !Showing))
            {
                if (animated)
                {
                    animating = true;
                    UIView.BeginAnimations(name);
                    UIView.SetAnimationDelegate(this);
                    UIView.SetAnimationBeginsFromCurrentState(true);
                    UIView.SetAnimationDidStopSelector(new ObjCRuntime.Selector("AnimationDidStop"));
                }

                if (hide)
                {
                    Center = hidingCenter;
                    Alpha = 0;
                }
                else
                {
                    Alpha = 1;
                    activityIndicator?.StartAnimating();
                    Center = showingCenter;
                }

                PlaceOnGrid();

                if (animated)
                {
                    UIView.CommitAnimations();
                }

                if (label != null)
                    label.Hidden = hide;
            }
        }

        #endregion

        #region Animations

        private void AnimationDidStop(string? animationId, bool finished)
        {
            if (!string.IsNullOrEmpty(animationId))
            {
                if (animationId.Equals(OHHideAnimation))
                {
                    activityIndicator?.StopAnimating();
                }
                else if (animationId.Equals(OHChangeProperty))
                {
                    var showName = OHShowAnimation;

                    if (animationDict != null)
                    {
                        string key;
                        foreach (var item in animationDict.Keys)
                        {
                            if (item != null)
                            {
                                var itemStr = item.ToString();
                                if (!string.IsNullOrEmpty(itemStr))
                                {
                                    key = itemStr;
                                }
                                else
                                {
                                    continue;
                                }

                                if (key == OHDiscreetNotificationViewActivityKey)
                                {
                                    showActivity = (animationDict?[key] as NSNumber)?.BoolValue ?? false;
                                }
                                else if (key == OHDiscreetNotificationViewTextKey)
                                {
                                    textLabel = (animationDict?[key].ToString());
                                }
                            }
                        }

                        animationDict = null;

                        Show(true, name: showName);
                    }
                }
                else if (animationId.Equals(OHShowAnimation))
                {
                    if (animationDict != null)
                    {
                        Hide(true, name: OHChangeProperty);
                    }
                }
            }

            animating = false;
        }

        #endregion

        #region Animated Setters

        //Change properties in an animated fashion
        //If you need to change properties, you need to use these methods. Hiding, changing value, and show it back will NOT work.

        public void SetTextLabel(string aText, bool animated)
        {
            if (animated && (Showing || animating))
            {
                ChangePropertyAnimated(new object[] { OHDiscreetNotificationViewTextKey }, new object[] { aText });
            }
            else
            {
                textLabel = aText;
            }
        }

        public void SetShowActivity(bool activity, bool animated)
        {
            if (animated && (Showing || animating))
            {
                ChangePropertyAnimated(new object[] { OHDiscreetNotificationViewActivityKey }, new object[] { activity ? 1 : 0 });
            }
            else
            {
                showActivity = activity;
            }
        }

        #endregion

        #region Helpers

        private void PlaceOnGrid()
        {
            var frame = this.Frame;

            frame.X = (nfloat)Math.Round(frame.X);
            frame.Y = (nfloat)Math.Round(frame.Y);

            this.Frame = frame;
        }

        private void ChangePropertyAnimated(object[]? keys, object[]? values)
        {
            NSDictionary? newDict = null;
            if (keys != null && values != null)
            {
                newDict = NSDictionary.FromObjectsAndKeys(values, keys);
            }

            if (animationDict == null)
            {
                animationDict = newDict;
            }
            else
            {
                var mutableAnimationDict = animationDict;
                if (newDict != null)
                {
                    foreach (var keyValuePair in newDict)
                    {
                        mutableAnimationDict[keyValuePair.Key] = keyValuePair.Value;
                        animationDict = mutableAnimationDict;
                    }
                }
            }

            if (!animating)
            {
                Hide(true, name: OHChangeProperty);
            }
        }

        #endregion

        #region UIView subclass

        public override void WillMoveToSuperview(UIView? newSuperview)
        {
            if (newSuperview == null)
            {
                animationDict = null;
            }
        }

        #endregion

        #region Dispose

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (View != null)
            {
                View.Dispose();
                View = null;
            }

            if (Label != null)
            {
                Label.Dispose();
                Label = null;
            }

            if (activityIndicator != null)
            {
                activityIndicator.Dispose();
                activityIndicator = null;
            }
        }

        #endregion
    }
}