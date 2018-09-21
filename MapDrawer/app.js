function getPathForTransportVehicle(transportType, vehicleType) {
    switch (transportType) {
        case "Tram":
            return "M10 0 L0 45 L45 45 L35 0 Z";
        case "Train":
            return "M0 0 L0 45 L45 45 L45 0 Z";
        case "Metro":
            return "M 22.5,22.5 m -22.5,0 a 22.5,22.5 0 1,0 45,0 a 22.5,22.5 0 1,0 -45,0";
        case "Monorail":
            return "M 0 8 A 8 8 0 0 1 8 0 L 37 0 A 8 8 0 0 1 45 8 L 45 37 A 8 8 0 0 1 37 45 L 8 45 A 8 8 0 0 1 0 37 Z";
        case "Ship":
            if (vehicleType === "Ship") return "M22.5 0 L45 22.5 L22.5 45 L0 22.5 Z";
            else return "M22.5 0 L45 22.5 L22.5 45 L0 22.5 ZM 37.5 7.5 L37.5 37.5 L7.5 37.5 L7.5 7.5 Z";
        case "Airplane":
            if (vehicleType === "Plane") return "M22.5,0 L45,16.5 L36.5,42.75 L8.75,42.75 L0,16.5 Z";
            else return "M22.5,45 L45,15 A 22.5 15 0 0 0 0 15 Z";
        case "Bus":
            return "M45,22.5 L35,45 L10,45 L0,22.5 L10,00 L35,00 Z";
    }
}



function getLineBlockString(lineObj) {
    return `<div class="lineBlock _lid_${lineObj.lineId}" style="--lineColor: ${lineObj.lineColor}">` +
        `<div class="lineNumber lineNumber"><svg width="45" height="45" xmlns="http://www.w3.org/2000/svg"><path d="${getPathForTransportVehicle(lineObj.transportType, lineObj.vehicleType)}" style="fill: var(--lineColor)"/><text lengthAdjust="${lineObj.lineStringIdentifier.length > 2 ? "spacingAndGlyphs":"spacing"}" x="22.5" y="22.5" textLength="42">${lineObj.lineStringIdentifier}</text></svg></div>` +
        `</div>`;
}

/*activeDay: true
​​
activeNight: true
​​
lineColor: "#6FE9B3"
​​
lineId: 40
​​
lineName: "[3104] ㎡ Britania - ㎡ Di Saboia"
​​
lineNumber: 3104
​​
lineStringIdentifier: "3104"
​​
simmetryRange: null
​​
stations: Array(13) [ {…}, {…}, {…}, … ]
​​
subservice: "PublicTransportTram"
​​
transportType: "Tram"
​​
vehicleType: "Tram"*/

$(document).ready(function () {
    $("#linesPanel #content").html("");
    var lines = Object.values(_infoLines.transportLines);
    lines.sort(function (a, b) {
        if (a.transportType === b.transportType) {
            if (a.vehicleType === b.vehicleType) {
                return a.lineStringIdentifier.localeCompare(b.lineStringIdentifier)
            }
            return a.vehicleType.localeCompare(b.vehicleType)
        }
        return a.transportType.localeCompare(b.transportType)
    });
    for (var x in lines) {
        $("#linesPanel #content").append(getLineBlockString(lines[x]))
    }
});