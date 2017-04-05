/*jslint browser: true, devel: true, node: true, sloppy: true, regexp: true */
/*global $, CJS */

"use strict";

var ChoixNomClient = function (serveur) {
    var me = this;
    
    this.nomChoisi = "";
    this.nomChoisiTmp = "";
    this.serveur = serveur;
    this.listeClients = [];
    this.erreur = "";
    
    serveur.post(
        CJS.ACTION__GET_NOM_MACHINE,
        function (retour) {
            me.nomChoisiTmp = retour[CJS.PARAM__NOM_MACHINE];
            me.testNomTmp();
        }
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
            me.testNomTmp();
        }
    );
};

ChoixNomClient.prototype.testNomTmp = function (e, ctx) {
    var me = typeof ctx !== "undefined" ? ctx.choixNomClient : this,
        nomInValide = /[^a-zA-Z0-9 ]/.test(me.nomChoisiTmp) || me.nomChoisiTmp.trim() === "" || me.nomChoisiTmp.length < 3;
    
    if (nomInValide) {
        me.erreur = "Le nom choisi n'est pas valide !";
    } else if (me.listeClients.indexOf(me.nomChoisiTmp.toLowerCase()) > -1) {
        me.erreur = "Le nom choisi est déjà utilisé par un autre client !";
    } else {
        me.erreur = "";
    }
};

ChoixNomClient.prototype.valideNomClient = function (e, ctx) {
    e.preventDefault();
    
    var me = typeof ctx !== "undefined" ? ctx.choixNomClient : this,
        params = {};
    
    params[CJS.PARAM__NOM_MACHINE] = me.nomChoisiTmp;
    me.serveur.post(
        CJS.ACTION__CHANGE_NOM_CLIENT,
        params,
        function () {
            me.nomChoisi = me.nomChoisiTmp;
            setTimeout(function () { ctx.gestionSynchros.getSynchros(); }, 500);
        }
    );
};