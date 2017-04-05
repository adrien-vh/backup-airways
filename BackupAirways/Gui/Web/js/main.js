/*jslint browser: true, devel: true, node: true, sloppy: true, regexp: true */
/*global $, CJS, Mustache, Serveur, rivets, GestionSynchros, GuiSynchros, ChoixDossierTampon, ChoixNomClient */

"use strict";

$(function () {
    var serveur = new Serveur("http://localhost:8000/"),
        gestionSynchros = new GestionSynchros(serveur),
        choixDossierTampon = new ChoixDossierTampon(serveur),
        choixNomClient = new ChoixNomClient(serveur);

    serveur.post(
        CJS.ACTION__ETAT_INITIALISATION,
        function (etat) {
            if (etat[CJS.PARAM__EST_INITIALISE]) {
                choixDossierTampon.dossierChoisi = etat[CJS.PARAM__DOSSIER_TAMPON];
                choixNomClient.nomChoisi = etat[CJS.PARAM__NOM_CLIENT];
            }
        }
    );

    rivets.bind(
        $('#contenantGlobal'),
        {
            choixDossierTampon: choixDossierTampon,
            choixNomClient: choixNomClient,
            gestionSynchros: gestionSynchros,
            gui: GuiSynchros
        }
    );
    
    gestionSynchros.getSynchros();
});