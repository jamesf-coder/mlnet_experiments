#!/bin/bash


# generate rectangles
for ((i=1; i<=100; i++))
do
    random_col1=$(printf "#%06x\n" $((RANDOM % 256 << 16 | RANDOM % 256 << 8 | RANDOM % 256)))
    random_col2=$(printf "#%06x\n" $((RANDOM % 256 << 16 | RANDOM % 256 << 8 | RANDOM % 256)))

    # convert -size 128x128 xc:${colours[colindex]} -fill white -draw "rectangle $((RANDOM % 129)),$((RANDOM % 129)) $((RANDOM % 129)),$((RANDOM % 129))" rectangles/draw_rect_$i.png
    convert -size 128x128 xc:$random_col1 -fill $random_col2 -draw "rectangle $((RANDOM % 129)),$((RANDOM % 129)) $((RANDOM % 129)),$((RANDOM % 129))" rectangles/draw_rect_$i.png

done

# generate circles
for ((i=1; i<=100; i++))
do
    random_col1=$(printf "#%06x\n" $((RANDOM % 256 << 16 | RANDOM % 256 << 8 | RANDOM % 256)))
    random_col2=$(printf "#%06x\n" $((RANDOM % 256 << 16 | RANDOM % 256 << 8 | RANDOM % 256)))

    convert -size 128x128 xc:$random_col1 -fill $random_col2 -draw "circle $((RANDOM % 129)),$((RANDOM % 129)) $((RANDOM % 129)),$((RANDOM % 129))" circles/draw_circle_$i.png

done

# generate triangles
for ((i=1; i<=100; i++))
do
    random_col1=$(printf "#%06x\n" $((RANDOM % 256 << 16 | RANDOM % 256 << 8 | RANDOM % 256)))
    random_col2=$(printf "#%06x\n" $((RANDOM % 256 << 16 | RANDOM % 256 << 8 | RANDOM % 256)))

    convert -size 128x128 xc:$random_col1 -fill $random_col2 -draw "path 'M $((RANDOM % 129)),$((RANDOM % 129))  L $((RANDOM % 129)),$((RANDOM % 129))  L $((RANDOM % 129)),$((RANDOM % 129)) L $((RANDOM % 129)),$((RANDOM % 129)) Z'" triangles/draw_triangle_$i.png

done
