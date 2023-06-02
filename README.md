# OHDiscreetNotification
A discreet notification view written in C#

![Demo](https://github.com/onurhazar/OHDiscreetNotification/blob/master/screenshot.png)

## Requirements
iOS 13+

## How to Use
```C#
var ohDiscreetNotification = new OHDiscreetNotification(new CGRect(30, 246, 315, 175))
{
    ThumbTintColor = UIColor.Brown,
    OnThumbTintColor = UIColor.Purple,
    ShadowColor = UIColor.LightGray
};
View.BackgroundColor = UIColor.Orange;
View.AddSubview(ohToggle);

```
