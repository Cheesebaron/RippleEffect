RippleEffect
============

This is a port from Java to Xamarin.Android C# of [RippleEffect](https://github.com/traex/RippleEffect),
which allows you to give any kind of view the Material design ripple effect when touching a view.

Simply wrap your view inside of `RippleView` and use the many options to control the animation and
rejoice with a nice ripple effect.

Usage
-----

```
<cheesebaron.rippleeffect.RippleView
    android:id="@+id/more"
    android:layout_width="?android:actionBarSize"
    android:layout_height="?android:actionBarSize"
    android:layout_toLeftOf="@+id/more2"
    android:layout_margin="5dp"
    ripple:rvCentered="true">
    <ImageView
        android:layout_width="?android:actionBarSize"
        android:layout_height="?android:actionBarSize"
        android:src="@android:drawable/ic_menu_edit"
        android:layout_centerInParent="true"
        android:padding="10dp"
        android:background="@android:color/holo_blue_dark"/>
</cheesebaron.rippleeffect.RippleView>
```

Customization
-------------

There are several attributes you can change in the XML declaration:

- rvAlpha: int between 0-255, default 90 - Alpha value of the ripple
- rvFramerate: int, default 10 - Frame rate of the ripple animation
- rvRippleDuration: int, default 400 - Duration of the ripple animation in ms
- rvColor: color, default white - Color of the ripple
- rvCentered: bool, default false - Center ripple in the child view
- rvType: enum, (simpleRipple, doubleRipple), default simpleRipple - Simple or double ripple
- rvZoom: bool, default false - Enables a zoom animation when true
- rvZoomDuration: int, default 150 - Duration of the zoom animation

Caveats
-------

When using double ripple a background needs to be set for the RippleView or for its child.

Thanks
------

Thanks to [Robin Chutaux](https://github.com/traex) for creating this library to begin with :)

License
-------

This project is licensed under the MIT License (MIT), please look at the LICENSE file in the repository.