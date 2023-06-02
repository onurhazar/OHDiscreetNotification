# OHDiscreetNotification
A discreet notification view written in C#

![Demo](https://github.com/onurhazar/OHDiscreetNotification/blob/master/screenshot.png)

## Requirements
iOS 13+

## How to Use
```C#
var discreetNotification = new OHDiscreetNotificationView("Synchronizing..", false, OHDNPresentationMode.Top, this.View);
discreetNotification.Show(true);

```
