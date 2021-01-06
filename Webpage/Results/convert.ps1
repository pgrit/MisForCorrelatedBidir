$methods = ls . | ?{$_.PSISContainer}
foreach ($m in $methods) {
    cd $m.FullName
    $scenes = ls . | ?{$_.PSISContainer}
    foreach ($s in $scenes) {
        cd $s.FullName
        magick mogrify -format jpg -quality 85 -sampling-factor 1x1 *.png
    }
    cd ..
}
