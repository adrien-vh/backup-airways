/*jslint browser: true, devel: true, node: true, sloppy: true, regexp: true */
/*global $, rivets */

"use strict";

rivets.formatters.eq = function (value1, value2) { return value1 === value2; };
rivets.formatters.neq = function (value1, value2) { return value1 !== value2; };
rivets.formatters.gt = function (value1, value2) { return value1 > value2; };
rivets.formatters.minus = function (value1, value2) { return value1 - value2; };
rivets.formatters.vide = function (value1) { return $.isArray(value1) ? value1.length === 0 : value1.trim() === ""; };
rivets.formatters.nonVide = function (value1) { return $.isArray(value1) ? value1.length !== 0 : value1.trim() !== ""; };
rivets.formatters.prefixe = function (value1, value2) { return value2 + value1; };