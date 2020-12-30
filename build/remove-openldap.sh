set -e # exit on error
killall slapd || true
sudo apt-get purge slapd -y
rm /tmp/slapd -r -f