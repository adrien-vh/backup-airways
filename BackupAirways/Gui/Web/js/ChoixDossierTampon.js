/*jslint browser: true, devel: true, node: true, sloppy: true, regexp: true */
/*global $, CJS */

"use strict";

var ChoixDossierTampon = function (serveur) {
    var me = this;
    
    this.serveur = serveur;
    
    this.dossierChoisi = "";
    this.dossierGoogleDrive = "";
    this.dossierOneDrive = "";
    this.dossierDropbox = "";
    this.dossierAutre = { val : "" };
    this.dossierChoisiTmp = { val : "" };
    
    
    serveur.post(CJS.ACTION__CHEMINS_DRIVES, function (data) {
        me.dossierGoogleDrive = data[CJS.PARAM__CHEMIN_GDRIVE] || "";
        me.dossierOneDrive = data[CJS.PARAM__CHEMIN_ONEDRIVE] || "";
        me.dossierDropbox = data[CJS.PARAM__CHEMIN_DROPBOX] || "";
    });
};


ChoixDossierTampon.prototype.valideDossierTampon = function (e, ctrl) {
    var params = {};

    params[CJS.PARAM__DOSSIER_TAMPON] = ctrl.choixDossierTampon.dossierChoisiTmp.val;
    
    ctrl.choixDossierTampon.serveur.post(
        CJS.ACTION__SET_DOSSIER_TAMPON,
        params,
        function () {
            ctrl.choixDossierTampon.dossierChoisi = ctrl.choixDossierTampon.dossierChoisiTmp.val;
            ctrl.choixNomClient.getNomsClientsExistants();
        }
    );
};

ChoixDossierTampon.prototype.dossierLocalChoisi = function (e, ctrl) {
    var me = typeof ctrl !== "undefined" ? ctrl.choixDossierTampon : this;
    
    me.dossierChoisiTmp.val = me.dossierAutre.val;
};