set -e # exit on error
killall slapd || true
sudo killall krb5kdc || true
sudo kdb5_util destroy -f || true
rm /tmp/krb5 -r -f
sudo apt-get purge slapd -y
rm /tmp/slapd -r -f