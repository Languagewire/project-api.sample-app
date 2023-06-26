!/bin/bash

for i in $(find ./ProjectApiSampleApp -name '*.cs');
do
  if ! grep -q copyright $i
  then
	sed -i '1s/^\xEF\xBB\xBF//' $i; 
	cat copyright.txt $i > $i.new && mv $i.new $i
  fi
done
