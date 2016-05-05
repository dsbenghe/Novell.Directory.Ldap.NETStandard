#!/bin/sh
which mcs  > /dev/null 2>&1
if [ $? != 0 ]
then
   echo "Unable to find C# compiler i.e mcs in the PATH."
   exit -1;
fi

if [ -d `pwd`/lib ]
then
   if [ -d `pwd`/doc ]
   then
	echo ""
   else
	mkdir -p `pwd`/doc
   fi

   echo -n "Checking for Old dll...."
	if [ -f `pwd`/lib/Novell.Directory.Ldap.dll ]
	then
		   echo ""
		   echo "dll already exists, removing Old `pwd`/lib/Novell.Directory.Ldap.dll"
	   	rm  `pwd`/lib/Novell.Directory.Ldap.dll
	else
		echo  "doesn't exist"
	fi
else
	echo "Creating `pwd`/lib directory "
	mkdir -p `pwd`/lib
	mkdir -p `pwd`/doc
fi

echo "Building resources..."
echo ""
	resgen Novell.Directory.Ldap/Novell.Directory.Ldap.Utilclass/ResultCodeMessages.txt lib/ResultCodeMessages.resources
	
	resgen Novell.Directory.Ldap/Novell.Directory.Ldap.Utilclass/ExceptionMessages.txt lib/ExceptionMessages.resources

   	echo "Building Novell.Directory.Ldap.dll..."
	echo ""
mcs -g /noconfig /target:library /r:mscorlib.dll /r:System.dll -resource:`pwd`/lib/ResultCodeMessages.resources -resource:`pwd`/lib/ExceptionMessages.resources -nowarn:0219,1570,1572,1574,1587 /doc:`pwd`/doc/comments.xml /out:`pwd`/lib/Novell.Directory.Ldap.dll @Novell.Directory.Ldap.dll.sources

if [ $? -ne 0 ]
then
echo " **************************************************************"
echo " If the error is \"Cannot find assembly mscorlib.dll\""
echo " It may be possible that you have installed an older version of mono"
echo " In older version of mono mscorlib.dll was named as corlib.dll"
echo " To solve this problem:"
echo " Replace /r:mscorlib.dll in script with /r:corlib.dll and execute"
echo " the build script once again"
echo " **************************************************************"
else
echo ""
echo "Novell.Directory.Ldap.dll generated in `pwd`/lib"
fi

rm `pwd`/lib/ResultCodeMessages.resources
rm `pwd`/lib/ExceptionMessages.resources
