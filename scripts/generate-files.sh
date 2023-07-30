count=$(shuf -i 5-10 -n 1)

echo Generating $count files with delays

for (( i=0 ; i<$count; i++))
do
    # echo creating file iteration $i
    sh -c ./create-test-file.sh
    sleep 3
done

echo Cleaning up files
cd ~/data/whatever
# pwd
rm test*