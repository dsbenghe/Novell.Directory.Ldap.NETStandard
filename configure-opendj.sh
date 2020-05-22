whoami
# run openjd in docker
docker run -d -h ldap-01.example.com -p 4389:1389 -p 4636:1636 -p 4444:4444 --name opendj --env-file opendj-docker-env.props openidentityplatform/opendj
# give openldap enough time to start
sleep 30
docker ps -a
# test to see that is running
ldapwhoami -H ldap://localhost:4389 -D "cn=admin,dc=example,dc=com" -w password 
ldapadd -h localhost:4389 -D cn=admin,dc=example,dc=com -w password -f test/conf/baseDn.ldif