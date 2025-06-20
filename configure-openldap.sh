set -e # exit on error
currentUser="$(whoami)"
ldapUser="openldap"
echo $currentUser
echo "slapd status"
#sudo apt-get install apparmor-utils -y
sudo service slapd stop
if grep -qEi "(microsoft|WSL)" /proc/version &> /dev/null ;  then
    # running under WSL/WSL2
    # apparmor doesnt seem to be active
    echo "Running under WSL"
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
sudo certtool -c --load-privkey /tmp/ssl/private/ldap_server.key --load-ca-certificate /tmp/ssl/certs/ca_server.pem --load-ca-privkey /tmp/ssl/private/ca_server.key --template test/conf/cert_template.conf --outfile /tmp/ssl/certs/ldap_server.pem
# # permissions
sudo usermod -aG ssl-cert $ldapUser
sudo chown $ldapUser:ssl-cert /tmp/ssl/private/ldap_server.key /tmp/ssl/certs/ldap_server.pem /tmp/ssl/certs/ca_server.pem
sudo chmod 640 /tmp/ssl/private/ldap_server.key /tmp/ssl/certs/ldap_server.pem /tmp/ssl/certs/ca_server.pem
# # end setup ssl
sudo chown -R $ldapUser:$ldapUser -v -c ./test/conf
echo "start slapd"
# slapd -f test/conf/slapd.conf -h "ldap://localhost:5389 ldaps://localhost:5636" -d -1 &
slapd -f ./test/conf/slapd.conf -h "ldap://localhost:5389 ldaps://localhost:5636" -d -1 
# give openldap enough time to start
sleep 5
# test to see that is running
echo "test slapd is running"
ldapwhoami -H ldap://localhost:5389 -D "cn=admin,dc=example,dc=com" -w password 
ldapadd -h localhost:5389 -D cn=admin,dc=example,dc=com -w password -f ./test/conf/setupData.ldif