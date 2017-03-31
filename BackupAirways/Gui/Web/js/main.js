/*jslint browser: true, devel: true, node: true, sloppy: true, regexp: true */
/*global $, CJS, Mustache, Serveur */

"use strict";

var LocalBrowserCtrl = function (destChemin, caller, onChange) {
    var me = this,
        uniqueId = "id" + Math.floor(10000 * Math.random());

    while ($("#" + uniqueId).length > 0) { uniqueId = "id" + Math.floor(10000 * Math.random()); };

    me.uniqueId = uniqueId;
    me.cheminChoisi = "";
    me.destChemin = destChemin;
    me.caller = caller;
    me.onChange = onChange;


    if ($("#_hoverBg").length === 0) { $("body").append('<div id="_hoverBg"></div>'); }

    if ($("#" + me.uniqueId).length === 0) {
        $("#_hoverBg").append('<div class="contenant-browser" id="' + me.uniqueId + '"><h2>Choisir un dossier local</h2><div class="browser"></div></div>');
        $("#" + me.uniqueId).LocalBrowser({
            onValide: function (chemin) {
                me.cheminChoisi = chemin;
                me.destChemin.val = chemin;
                $("#_hoverBg").hide();
                $("#" + me.uniqueId).hide();

                if (typeof me.caller !== "undefined" && typeof me.onChange !== "undefined") {
                    me.onChange.apply(caller);
                }
            },
            onAnnule: function () {
                $("#_hoverBg").hide();
                $("#" + me.uniqueId).hide();
            }
        });
        $("#" + me.uniqueId).hide();
    }
};

LocalBrowserCtrl.prototype.showBrowser = function (e, localBrowserCtrl) {
    e.preventDefault();
    $("#_hoverBg").show();
    $("#" + localBrowserCtrl.uniqueId).show();
};

// Composant « Browser Local »
rivets.components['local-browser'] = {
    template: function () {
        return '<a href="#" rv-on-click="showBrowser" class="bout-choix-dossier">' +
            '<span rv-show="cheminChoisi | nonVide">{ cheminChoisi }</span>' +
            '<span rv-show="cheminChoisi | vide">(aucun dossier choisi)</span> ' +
            '<span class="bouton">Parcourir...</span>' +
            '</a>';
    },

    initialize: function (el, data) {
        console.log(data);
        return new LocalBrowserCtrl(data.chemin, data.caller, data.onChange);
    }
}

rivets.formatters.eq = function (value1, value2) { return value1 === value2; }
rivets.formatters.neq = function (value1, value2) { return value1 !== value2; }
rivets.formatters.gt = function (value1, value2) { return value1 > value2; }
rivets.formatters.minus = function (value1, value2) { return value1 - value2; }
rivets.formatters.vide = function (value1) { return $.isArray(value1) ? value1.length === 0 : value1.trim() === ""; }
rivets.formatters.nonVide = function (value1) { return $.isArray(value1) ? value1.length !== 0 : value1.trim() !== ""; }
rivets.formatters.prefixe = function (value1, value2) { return value2 + value1; }

var GestionSynchros = function (serveur) {
    this.synchros = [],
    this.nomMachine = ""
    this.serveur = serveur;
    this.nouvelleSynchro = {
        chemin : { val : "" },
        nom : "",
        erreur : ""
    };

    this.valideNouvelleSynchro();
};

GestionSynchros.prototype.valideNouvelleSynchro = function (e, ctrl) {
    var me = typeof ctrl !== "undefined" ? ctrl.gestionSynchros : this;

    me.nouvelleSynchro.nom = me.nouvelleSynchro.nom.trim();
    var nomInValide = /[^a-zA-Z0-9 ]/.test(me.nouvelleSynchro.nom) || me.nouvelleSynchro.nom.trim() === "" || me.nouvelleSynchro.nom.length < 3,
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
    )
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
}

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
}

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
}

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
}

function main() {
    var gestionSynchros = new GestionSynchros(new Serveur("http://localhost:8000/")),
        gui = {};

    // Gestion de l'interface utilisateurs
    gui.montreSousContenu = function (e) {
        e.preventDefault();
        var hauteur = $(this).parents(".sauvegarde").find(".contenant").height(),
            ref = $(this).attr("data-ref");

        $(this).parents(".sauvegarde").find(".sous-contenu." + ref).detach().insertAfter($(this).parents(".sauvegarde").find(".sous-contenu.existant"));

        $(this).parents(".sauvegarde").find(".cont-bouton.existant").hide();
        $(this).parents(".sauvegarde").find(".cont-bouton." + ref).show();

        $(this).parents(".sauvegarde").find(".contenu").animate({ marginTop: "-" + hauteur + "px" }, 300);
    };

    gui.cacheSousContenu = function (e) {
        e.preventDefault();
        var hauteur = $(this).parents(".sauvegarde").find(".contenant").height(),
            ref = $(this).attr("data-ref");

        $(this).parents(".sauvegarde").find(".cont-bouton").hide();
        $(this).parents(".sauvegarde").find(".cont-bouton.existant").show();

        $(this).parents(".sauvegarde").find(".contenu").animate({ marginTop: "0px" }, 300);
    };


    // Récupération des synchros depuis le serveur

    rivets.bind($('#gestionSynchros'), { gestionSynchros: gestionSynchros, gui: gui });
    gestionSynchros.getSynchros();

}

$(function () {

    main();

    var params = {},
        cloudsAffiches = true,
        listeClients = [],
        serveur = new Serveur("http://localhost:8000/"),
        nomClient = "";

    function formatteSynchro(synchro) {
        var i;

        for (i = 0; i < synchro.Clients.length; i += 1) {
            synchro.Clients[i].deltaFichiers = synchro.NbFichiersMaitre - synchro.Clients[i].NbFichiers;
            synchro.Clients[i].complete = (synchro.Clients[i].deltaFichiers === 0);
            synchro.Clients[i].estClientLocal = synchro.Clients[i].Client === nomClient;

        }
    }

    function recupSauvegardes() {
        serveur.post(
            CJS.ACTION__LISTE_SYNCHROS,
            function (sauvegardes) {
                var template = $('#tplSauvegarde').html(), rendered, i, j, synchros = sauvegardes[CJS.PARAM__SYNCHROS_MAITRES].concat(sauvegardes[CJS.PARAM__SYNCHROS_ESCLAVES]).concat(sauvegardes[CJS.PARAM__SYNCHROS_INUTILISEES]), synchro;

                nomClient = sauvegardes[CJS.PARAM__NOM_MACHINE];

                //Mustache.parse(template);
                $("#listeSauvegardes .sauvegarde.esclave").remove();
                $("#listeSauvegardes .sauvegarde.maitre").remove();
                $("#listeSauvegardes .sauvegarde.inutilisee").remove();

                template = $('#tplSynchro').html();

                for (i = 0; i < synchros.length; i += 1) {
                    synchro = synchros[i];
                    synchro.classe = synchro.Type === 1 ? "maitre " : (synchro.Type === 0 ? "esclave " : "inutilisee ");
                    synchro.classe += synchro.Valide ? "actif" : "inactif";
                    synchro.maitre = (synchro.Type === 1);
                    synchro.esclave = (synchro.Type === 0);
                    synchro.inutilisee = (synchro.Type === 2);
                    formatteSynchro(synchro);
                    rendered = Mustache.render(template, synchro);
                    $('#listeSauvegardes').append(rendered);
                }

                $(".sauvegarde.inutilisee .dossier-local").ChoixDossier({
                    onChange: function (chemin) {
                        $(this).parents(".sauvegarde").find(".cont-bouton.recuperation .confirme-recuperation").toggleClass("inactif", chemin === "");
                        $(this).parents(".sauvegarde").find(".sous-contenu.recuperation .message-erreur").toggle(chemin === "");
                    }
                });

                $(".sauvegarde.inutilisee .confirme-recuperation").click(function (e) {
                    e.preventDefault();
                    var params = {};
                    params[CJS.PARAM__DOSSIER] = $(this).parents(".sauvegarde").find(".sous-contenu.recuperation .dossier-local").attr("data-chemin");
                    params[CJS.PARAM__NOM_SYNCHRO] = $(this).attr("data-synchro");

                    serveur.post(
                        CJS.ACTION__JOINDRE_SYNCHRO,
                        params,
                        function (retour) {
                            recupSauvegardes();
                        }
                    );

                    console.log(params);
                });

                $(".sauvegarde a.lien-sous-contenu").click(function (e) {
                    e.preventDefault();
                    var hauteur = $(this).parents(".sauvegarde").find(".contenant").height(),
                        ref = $(this).attr("data-ref");

                    $(this).parents(".sauvegarde").find(".sous-contenu." + ref).detach().insertAfter($(this).parents(".sauvegarde").find(".sous-contenu.existant"));

                    $(this).parents(".sauvegarde").find(".cont-bouton.existant").hide();
                    $(this).parents(".sauvegarde").find(".cont-bouton." + ref).show();

                    $(this).parents(".sauvegarde").find(".contenu").animate({ marginTop: "-" + hauteur + "px" }, 300);
                });

                $(".sauvegarde .cont-bouton a.annuler").click(function (e) {
                    e.preventDefault();
                    var hauteur = $(this).parents(".sauvegarde").find(".contenant").height(),
                        ref = $(this).attr("data-ref");

                    //$(this).parents(".sauvegarde").find(".sous-contenu." + ref).detach().insertAfter($(this).parents(".sauvegarde").find(".sous-contenu.existant"));

                    $(this).parents(".sauvegarde").find(".cont-bouton").hide();
                    $(this).parents(".sauvegarde").find(".cont-bouton.existant").show();


                    $(this).parents(".sauvegarde").find(".contenu").animate({ marginTop: "0px" }, 300);
                });

                $(".bouton-supprime-client").click(function (e) {
                    $(this).parents(".sauvegarde").find(".cont-bouton.fin-recuperation .confirme-fin-recuperation").attr("data-client", $(this).attr("data-client"));
                    $(this).parents(".sauvegarde").find(".sous-contenu.fin-recuperation .nom-client").html($(this).attr("data-client"));
                });

                $(".sauvegarde .confirme-suppression").click(function (e) {
                    e.preventDefault();

                    var params = {};
                    params[CJS.PARAM__NOM_SYNCHRO] = $(this).attr("data-synchro");

                    serveur.post(
                        CJS.ACTION__SUPPRIME_SYNCHRO,
                        params,
                        function () {
                            recupSauvegardes();
                        }
                    );
                });

                $(".sauvegarde .confirme-fin-recuperation").click(function (e) {
                    e.preventDefault();
                    var params = {};
                    params[CJS.PARAM__NOM_SYNCHRO] = $(this).attr("data-synchro");
                    params[CJS.PARAM__NOM_MACHINE] = $(this).attr("data-client");

                    serveur.post(
                        CJS.ACTION__SUPPRIME_CLIENT_SYNCHRO,
                        params,
                        function () {
                            recupSauvegardes();
                        }
                    );
                });
            }
        );
    }


    // Validation du nom de la machine
    function valideNomMachine() {
        $("#inNomMachine").val($("#inNomMachine").val().trim());
        var nomInValide = /[^a-zA-Z0-9]/.test($("#inNomMachine").val()) || $("#inNomMachine").val().trim() === "",
            nomExistant = listeClients.indexOf($("#inNomMachine").val().toLowerCase()) !== -1;

        $("#boutConfirmeNomMachine").toggleClass("inactif", nomInValide || nomExistant);

        if (nomInValide) {
            $("#erreurNomClient").html("Le nom choisi n'est pas valide.");
            $("#erreurNomClient").show();
        } else if (nomExistant) {
            $("#erreurNomClient").html("Un client porte déjà ce nom.");
            $("#erreurNomClient").show();
        } else {
            $("#erreurNomClient").hide();
        }
    }

    // Récupération de la liste des clients
    function recupClients() {
        serveur.post(
            CJS.ACTION__GET_LISTE_CLIENTS,
            function (clients) {
                var i;
                for (i = 0; i < clients.length; i += 1) { clients[i] = clients[i].toLowerCase(); }
                listeClients = clients;
                valideNomMachine();
            }
        );
    }

    $("#inNomSauvegarde").val("");

    // Récupération de l'état de l'application
    serveur.post(
        CJS.ACTION__ETAT_INITIALISATION,
        function (etat) {
            $(".page").hide();
            if (etat[CJS.PARAM__EST_INITIALISE]) {
                $("#pageListeSauvegardes").show();
                //$("#pageChoixNom").show();
                //$("#pageChoixTampon").show();
                //recupSauvegardes();
            } else {
                $("#pageChoixTampon").show();
                /**/
                // recupClients();
                //    $("#pageChoixNom").show();
                /**/
            }
        }
    );



    /*****************************************************************************/
    /* -== PAGE CHOIX DU DOSSIER TAMPON ==-                                      */

    // Récupération des chemins vers les clouds installés sur le poste
    serveur.post(CJS.ACTION__CHEMINS_DRIVES, function (data) {
        if (typeof data[CJS.REP__CHEMIN_GDRIVE] !== "undefined") {
            $(".choix-cloud span.gdrive.inactif").hide();
            $(".choix-cloud span.gdrive.actif").css("display", "inline-block");
            $(".choix-cloud span.gdrive .chemin").html(data[CJS.REP__CHEMIN_GDRIVE]);
        }
    });

    // Mise en place du bouton parcourir
    $("#pageChoixTampon .dossier-local").ChoixDossier({ onChange: function (chemin) { $("#pageChoixTampon .autre-cloud .chemin").html(chemin); $("#boutConfirmeDossierTampon").toggleClass("inactif", chemin === ""); } });

    // Au changement de choix de cloud
    $(".choix-cloud span.actif").click(function (e) {
        $(".choix-cloud span.actif.choisi").removeClass("choisi");
        $(this).addClass("choisi");
        $("#boutConfirmeDossierTampon").toggleClass("inactif", $(".choix-cloud span.actif.choisi .chemin").html() === "");
    });

    // À la confirmation du dossier tampon
    $("#boutConfirmeDossierTampon").click(function (e) {
        e.preventDefault();

        var params = {};

        params[CJS.PARAM__DOSSIER_TAMPON] = $(".choix-cloud span.actif.choisi .chemin").html();

        serveur.post(
            CJS.ACTION__SET_DOSSIER_TAMPON,
            params,
            function () {
                $(".page").hide();
                $("#pageChoixNom").show();
                //recupSauvegardes();
                recupClients();
            }
        );
    });

    /* -== FIN PAGE CHOIX DU DOSSIER TAMPON ==-                                  */
    /*****************************************************************************/

    /*****************************************************************************/
    /* -== PAGE CHOIX DU NOM DU CLIENT ==-                                       */


    // Récupération du nom de la machine
    serveur.post(
        CJS.ACTION__GET_NOM_MACHINE,
        function (retour) { $("#inNomMachine").val(retour[CJS.PARAM__NOM_MACHINE]); }
    );

    $("#inNomMachine").keyup(function (e) {
        e.preventDefault();
        valideNomMachine();
    });

    $("#boutConfirmeNomMachine").click(function (e) {
        e.preventDefault();
        var params = {};
        params[CJS.PARAM__NOM_MACHINE] = $("#inNomMachine").val();
        serveur.post(
            CJS.ACTION__CHANGE_NOM_CLIENT,
            params,
            function () {
                recupSauvegardes();
                $(".page").hide();
                $("#pageListeSauvegardes").show();
            }
        );
    });

    /* -== FIN PAGE CHOIX DU DOSSIER TAMPON ==-                                  */
    /*****************************************************************************/

    /*****************************************************************************/
    /* -== PAGE LISTE DES SAUVEGARDES ==-                                        */

    // Validation du nom de la nouvelle sauvegarde
    function valideNomSauvegarde() {
        $("#inNomSauvegarde").val($("#inNomSauvegarde").val().trim());
        var nomInValide = /[^a-zA-Z0-9 ]/.test($("#inNomSauvegarde").val()) || $("#inNomSauvegarde").val().trim() === "" || $("#inNomSauvegarde").val().length < 3,
            cheminInvalide = $("#dossierASauvegarder").attr("data-chemin") ? false : true;

        $("#boutNouvelleSauvegarde").toggleClass("inactif", nomInValide || cheminInvalide);

        if (nomInValide) {
            $("#erreurNouvelleSauvegarde span").html("Le nom choisi n'est pas valide (au moins 3 caractères sans caractères spéciaux).");
            $("#erreurNouvelleSauvegarde").show();
        } else if (cheminInvalide) {
            $("#erreurNouvelleSauvegarde span").html("Le dossier choisi n'est pas valide.");
            $("#erreurNouvelleSauvegarde").show();
        } else {
            $("#erreurNouvelleSauvegarde").hide();
        }
    }

    /*valideNomSauvegarde();

    $("#inNomSauvegarde").keyup(function (e) {
        e.preventDefault();
        valideNomSauvegarde();
    });

    $("#dossierASauvegarder").ChoixDossier({ onChange: function () { valideNomSauvegarde(); } });

    $("#boutNouvelleSauvegarde").click(function (e) {
        e.preventDefault();
        var params = {};
        params[CJS.PARAM__DOSSIER] = $("#dossierASauvegarder").attr("data-chemin");
        params[CJS.PARAM__NOM_SYNCHRO] = $("#inNomSauvegarde").val();
        serveur.post(
            CJS.ACTION__NOUVELLE_SYNCHRO,
            params,
            function () {
                $("#inNomSauvegarde").val("");
                $(".sauvegarde.ajout .cont-bouton.form").hide();
                $(".sauvegarde.ajout .cont-bouton.existant").show();
                $(".sauvegarde.ajout .contenu").css("margin-top", 0);
                recupSauvegardes();
            }
        );
    });*/

    /* -== FIN PAGE LISTE DES SAUVEGARDES ==-                                    */
    /*****************************************************************************/


});