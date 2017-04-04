/*jslint browser: true, devel: true, node: true, sloppy: true, regexp: true */
/*global $, CJS */

"use strict";

var ChoixNomClient = function (serveur) {
    var me = this;
    
    this.nomChoisi = "";
    this.nomChoisiTmp = "";
    this.serveur = serveur;
    this.listeClients = [];
    
    serveur.post(
        CJS.ACTION__GET_NOM_MACHINE,
        function (retour) { me.nomChoisiTmp = retour[CJS.PARAM__NOM_MACHINE]; }
    );
};

ChoixNomClient.prototype.getNomsClientsExistants = function () {
    var me = this;
    
    this.serveur.post(
        CJS.ACTION__GET_LISTE_CLIENTS,
        function (clients) {
            var i;
            for (i = 0; i < clients.length; i += 1) { clients[i] = clients[i].toLowerCase(); }
            me.listeClients = clients;
        }
    );
};