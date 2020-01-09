https://carlosbecker.com/posts/selenium-grid-docker-compose/

https://shell.azure.com/

az account list
az account set --subscription EcoVadis-ITD-LAB

mkdir abmlab
cd $HOME/abmlab

 az group create --name abmrg --location "west europe"

az appservice plan create --name abmserviceplan --resource-group abmrg --sku S1 --is-linux

az webapp create --resource-group abmrg --plan abmserviceplan --name evseleniumgrid --multicontainer-config-type compose --multicontainer-config-file docker-compose.yaml

az webapp log tail --name evseleniumgrid  --resource-group abmrg 


docker-compose scale chrome=5
--scale chrome=5

https://github.com/SeleniumHQ/docker-selenium/wiki/Getting-Started-with-Docker-Compose