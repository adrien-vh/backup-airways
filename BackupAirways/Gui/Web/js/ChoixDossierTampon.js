/*jslint browser: true, devel: true, node: true, sloppy: true, regexp: true */
/*global $, CJS */

"use strict";

var ChoixDossierTampon = function (serveur) {
    var me = this;
    
    this.dossierChoisi = "";
    this.dossierGoogleDrive = "";
    this.dossierOneDrive = "";
    this.dossierDropbox = "";
    this.dossierChoisiTmp = { val : "" };
    
    
    serveur.post(CJS.ACTION__CHEMINS_DRIVES, function (data) {
        me.dossierGoogleDrive = data[CJS.PARAM__CHEMIN_GDRIVE] || "";
        me.dossierOneDrive = data[CJS.PARAM__CHEMIN_ONEDRIVE] || "";
        me.dossierDropbox = data[CJS.PARAM__CHEMIN_DROPBOX] || "";
    });
};