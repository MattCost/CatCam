delay=$(shuf -i 2-5 -n 1)
filename=$RANDOM

echo "file /home/matt/data/whatever/test-$filename over $delay seconds"
flock --exclusive --wait 5 /home/matt/data/whatever/test-$filename sh -c "echo hello world > /home/matt/data/whatever/test-$filename ; sleep $delay ; echo goodbye >> /home/matt/data/whatever/test-$filename; mv /home/matt/data/whatever/test-$filename /home/matt/data/whatever/test-$filename.txt"
