# ip-gps-finder
Repository temporaneo per api di gestione IP e coordinate GPS

## Obiettivi

* Realizzare un microservice che renda un API per:
   * il detect dell'IP del client, ne calcoli le coordinate GPS (lat, long), restituendo un oggetto in formato Json
   * se passati due punti GPS (lat1, long1), (lat2, long2) ritorni la distanza con cacolo GPS
   
   
* Realizzare le chiamate di prova e di test all'inderno dell'applicativo Postman, creando una documentazione delle stesse

* Informazioni accessorie:

  * Macchina virtuale di sviluppo accesso tramite client ssh e remote desktop all'indirizzo: ssh 5.189.187.198:5301 (utilizzare Bitvise SSH Client: https://www.bitvise.com/ssh-client-download)
  * URL per accetssot da remoto  alla Api: http://5.189.187.198:15301[END-POINT]  (es. http://5.189.187.198:15301/api/ipgpsfinder/v1/health)

