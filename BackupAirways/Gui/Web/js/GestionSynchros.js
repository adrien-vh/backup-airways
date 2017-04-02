/*jslint browser: true, devel: true, node: true, sloppy: true, regexp: true */
/*global $, CJS */

"use strict";

var GestionSynchros = function (serveur) {
    this.synchros = [];
    this.nomMachine = "";
    this.serveur = serveur;
    this.nouvelleSynchro = {
        chemin : { val : "" },
        nom : "",
        erreur : ""
    };

    this.valideNouvelleSynchro();
};

GestionSynchros.prototype.valideNouvelleSynchro = function (e, ctrl) {
    var me = typeof ctrl !== "undefined" ? ctrl.gestionSynchros : this,
        cheminInvalide,
        nomInValide;

    me.nouvelleSynchro.nom = me.nouvelleSynchro.nom.trim();
    nomInValide = /[^a-zA-Z0-9 ]/.test(me.nouvelleSynchro.nom) || me.nouvelleSynchro.nom.trim() === "" || me.nouvelleSynchro.nom.length < 3;
    cheminInvalide = me.nouvelleSynchro.chemin.val === "" ? true : false;

    if (nomInValide) {
        me.nouvelleSynchro.erreur = "Le nom choisi n'est pas valide (au moins 3 caractères sans caractères spéciaux).";
    } else if (cheminInvalide) {
        me.nouvelleSynchro.erreur = "Le dossier choisi n'est pas valide.";
    } else {
        me.nouvelleSynchro.erreur = "";
    }
};

GestionSynchros.prototype.getSynchros = function () {
    var me = this;

    me.serveur.post(
        CJS.ACTION__LISTE_SYNCHROS,
        function (synchrosRecuperees) {
            var i, synchros;

            synchros = synchrosRecuperees[CJS.PARAM__SYNCHROS_MAITRES]
                .concat(synchrosRecuperees[CJS.PARAM__SYNCHROS_ESCLAVES])
                .concat(synchrosRecuperees[CJS.PARAM__SYNCHROS_INUTILISEES]);

            for (i = 0; i < synchros.length; i += 1) {
                synchros[i].cheminRecuperation = { val: "" };
            }

            me.synchros = synchros;
            me.nomMachine = synchrosRecuperees[CJS.PARAM__NOM_MACHINE];

            $(".sauvegarde .contenu").css({ marginTop: "0px" });
        }
    );
};

GestionSynchros.prototype.joindreSynchro = function (synchro) {
    console.log(synchro);

    return function (e, ctrl) {
        e.preventDefault();

        console.log(ctrl);

        var params = {};

        params[CJS.PARAM__DOSSIER] = synchro.cheminRecuperation.val;
        params[CJS.PARAM__NOM_SYNCHRO] = synchro.Nom;

        ctrl.gestionSynchros.serveur.post(
            CJS.ACTION__JOINDRE_SYNCHRO,
            params,
            function () { ctrl.gestionSynchros.getSynchros(); }
        );
    };
};

GestionSynchros.prototype.quitterSynchro = function (synchro, client) {

    return function (e, ctrl) {
        e.preventDefault();

        var params = {};
        params[CJS.PARAM__NOM_SYNCHRO] = synchro.Nom;
        params[CJS.PARAM__NOM_MACHINE] = client;

        ctrl.gestionSynchros.serveur.post(
            CJS.ACTION__SUPPRIME_CLIENT_SYNCHRO,
            params,
            function () { ctrl.gestionSynchros.getSynchros(); }
        );

    };
};

GestionSynchros.prototype.creerSynchro = function (e, ctrl) {

    e.preventDefault();

    var params = {};

    params[CJS.PARAM__DOSSIER] = ctrl.gestionSynchros.nouvelleSynchro.chemin.val;
    params[CJS.PARAM__NOM_SYNCHRO] = ctrl.gestionSynchros.nouvelleSynchro.nom;

    console.log(params);

    ctrl.gestionSynchros.serveur.post(
        CJS.ACTION__NOUVELLE_SYNCHRO,
        params,
        function () {
            ctrl.gestionSynchros.nouvelleSynchro.nom = "";
            $(".sauvegarde.ajout .cont-bouton.form").hide();
            $(".sauvegarde.ajout .cont-bouton.existant").show();
            $(".sauvegarde.ajout .contenu").css("margin-top", 0);
            ctrl.gestionSynchros.getSynchros();
        }
    );
};

GestionSynchros.prototype.supprimerSynchro = function (synchro) {

    return function (e, ctrl) {
        e.preventDefault();

        var params = {};
        params[CJS.PARAM__NOM_SYNCHRO] = synchro.Nom;

        ctrl.gestionSynchros.serveur.post(
            CJS.ACTION__SUPPRIME_SYNCHRO,
            params,
            function () { ctrl.gestionSynchros.getSynchros(); }
        );

    };
};
