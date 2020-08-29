whoami
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
# permissions
sudo usermod -aG ssl-cert travis
sudo chown travis:ssl-cert /tmp/ssl/private/ldap_server.key /tmp/ssl/certs/ldap_server.pem /tmp/ssl/certs/ca_server.pem
sudo chmod 640 /tmp/ssl/private/ldap_server.key /tmp/ssl/certs/ldap_server.pem /tmp/ssl/certs/ca_server.pem
# end setup ssl
# start ssl
slapd -f test/conf/slapd.conf -h "ldap://localhost:5389 ldaps://localhost:5636" &
# give openldap enough time to start
sleep 5
# test to see that is running
ldapwhoami -H ldap://localhost:5389 -D "cn=admin,dc=example,dc=com" -w password 
ldapadd -h localhost:5389 -D cn=admin,dc=example,dc=com -w password -f test/conf/baseDn.ldif