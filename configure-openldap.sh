set -e # exit on error
currentUser="$(whoami)"
echo "$currentUser"
echo "slapd status"
sudo apt-get install apparmor-utils -y
sudo aa-status
sudo service slapd stop

if grep -qEi "(microsoft|WSL)" /proc/version &> /dev/null ;  then
    # running under WSL/WSL2
    # apparmor doesnt seem to be active
    echo "Running under WSL"
else
    # disable apparmor for slapd
    # sudo aa-disable slapd
    ls -la /etc/apparmor.d/
    echo "disable aa slapd"
    sudo ln -s /etc/apparmor.d/usr.sbin.slapd /etc/apparmor.d/disable/
    sudo apparmor_parser -R /etc/apparmor.d/usr.sbin.slapd
    echo "end disable aa slapd"
fi
# work folder for slapd
mkdir /tmp/slapd
# start setup ssl
# prepare folders
mkdir -p /tmp/ssl/private
mkdir -p /tmp/ssl/certs
# generate certs/keys
sudo certtool -p --outfile /tmp/ssl/private/ca_server.key
sudo certtool -s --load-privkey /tmp/ssl/private/ca_server.key --template test/conf/cert_template.conf --outfile /tmp/ssl/certs/ca_server.pem
sudo certtool -p --sec-param low --outfile /tmp/ssl/private/ldap_server.key
sudo certtool -c --load-privkey /tmp/ssl/private/ldap_server.key --load-ca-certificate /tmp/ssl/certs/ca_server.pem --load-ca-privkey /tmp/ssl/private/ca_server.key --template ./test/conf/cert_template.conf --outfile /tmp/ssl/certs/ldap_server.pem
# # permissions
sudo usermod -aG ssl-cert "$currentUser"
sudo chown "$currentUser":ssl-cert /tmp/ssl/private/ldap_server.key /tmp/ssl/certs/ldap_server.pem /tmp/ssl/certs/ca_server.pem
sudo chmod 777 -v -c /tmp/ssl/private/ldap_server.key /tmp/ssl/certs/ldap_server.pem /tmp/ssl/certs/ca_server.pem
# # end setup ssl
sudo chmod -R 777 -v -c ./test/conf
ls -la ./test/conf
echo "start slapd"
# slapd -f test/conf/slapd.conf -h "ldap://localhost:5389 ldaps://localhost:5636" -d -1 &
# Access denied here is most likely due to apparmour
slapd -u "$currentUser" -f ./test/conf/slapd.conf -h "ldap://localhost:5389 ldaps://localhost:5636" & # -d -1
# give openldap enough time to start
sleep 5
# test to see that is running
echo "test slapd is running"
ldapwhoami -H ldap://localhost:5389 -D "cn=admin,dc=example,dc=com" -w password 
ldapadd -H ldap://localhost:5389 -D cn=admin,dc=example,dc=com -w password -f ./test/conf/setupData.ldif