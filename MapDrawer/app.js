 var sizeMultiplier = 50;


        var stopsPositionList = {};
        for (let x in _infoLines.transportLines) {
            for (let s in _infoLines.transportLines[x].stations) {
                for (let y in _infoLines.transportLines[x].stations[s].stops) {
                    stopsPositionList[y] = [_infoLines.transportLines[x].stations[s].stops[y][0], _infoLines.transportLines[x].stations[s].stops[y][2]];
                }
            }
        }

        var minDistance = Infinity;
        for (let x in _infoLines.transportLines) {
            //  if (_infoLines.transportLines[x].transportType != "Train") continue;
            for (let s in _infoLines.transportLines[x].stations) {
                let currentStop = _infoLines.transportLines[x].stations[s].stopId;
                let nextStop = _infoLines.transportLines[x].stations[(s + 1) % _infoLines.transportLines[x].stations.length].stopId;
                dist = Math.sqrt(Math.pow(stopsPositionList[currentStop][0] - stopsPositionList[nextStop][0], 2) + Math.pow(stopsPositionList[currentStop][1] - stopsPositionList[nextStop][1], 2));
                if (dist < minDistance && dist > 8) {
                    minDistance = dist;
                }
            }
        }


        function SafeStore2(stopId, rx, ry) {
            let tx = ((stopsPositionList[stopId][0]) / minDistance + 0x8000) >> 0;
            let ty = ((stopsPositionList[stopId][1]) / minDistance + 0x8000) >> 0;
            stopToTgPos[stopId] = [tx, ty]
            if (tgPosToStop[tx] === undefined) { tgPosToStop[tx] = {} }
            tgPosToStop[tx][ty] = stopId;
            tgPosToRealX[tx] = stopsPositionList[stopId][0];
            tgPosToRealY[ty] = stopsPositionList[stopId][1];
        }

        function recalculate() {
            stopToTgPos = {};
            tgPosToStop = {};
            tgPosToRealX = {};
            tgPosToRealY = {};
            stopToMapPos = {};

            for (let x in _infoLines.transportLines) {
                //   if (_infoLines.transportLines[x].transportType != "Train") continue;
                for (let s in _infoLines.transportLines[x].stations) {
                    let stopId = _infoLines.transportLines[x].stations[s].stopId;
                    SafeStore2(stopId, stopsPositionList[stopId][0], stopsPositionList[stopId][1]);
                }
            }

            let xSortedKeys = Object.keys(tgPosToRealX).map(x => x * 1).sort(x => x);
            let ySortedKeys = Object.keys(tgPosToRealY).map(x => x * 1).sort(x => -x);

            for (let stop in stopToTgPos) {
                stopToMapPos[stop] = [xSortedKeys.indexOf(stopToTgPos[stop][0]), ySortedKeys.indexOf(stopToTgPos[stop][1])];
            }
            $(document).ready(function () {
                $("#map")[0].setAttribute('height', 240 + sizeMultiplier * ySortedKeys.length)
                $("#map")[0].setAttribute('width', 240 + sizeMultiplier * xSortedKeys.length)
            });
            return stopToMapPos;
        }
        var stopToMapPos = recalculate();

        var segments = [];

        function getNextItemArray(array, idx) {
            return array[(idx + 1) % array.length]
        }
        function getPrevItemArray(array, idx) {
            return array[(idx + array.length - 1) % array.length]
        }

        for (let x in _infoLines.transportLines) {
            // if (_infoLines.transportLines[x].transportType != "Train") continue;
            for (let s in _infoLines.transportLines[x].stations) {
                let currentStop = _infoLines.transportLines[x].stations[s].stopId;
                let nextStop = getNextItemArray(_infoLines.transportLines[x].stations, 1 * s).stopId;

                let exitOffset = 1;
                let enterOffset = 0;
                let exitStop = getNextItemArray(_infoLines.transportLines[x].stations, 1 * s + exitOffset).stopId;
                let enterStop = getPrevItemArray(_infoLines.transportLines[x].stations, 1 * s + enterOffset).stopId;
                let coord0 = stopToMapPos[enterStop];
                let coord1 = stopToMapPos[currentStop];
                let coord2 = stopToMapPos[nextStop];
                let coord3 = stopToMapPos[exitStop];
                let coord0Id = coord0[0] << 12 | coord0[1];
                let coord1Id = coord1[0] << 12 | coord1[1];
                let coord2Id = coord2[0] << 12 | coord2[1];
                let coord3Id = coord3[0] << 12 | coord3[1];

                while (coord0Id == coord1Id) { coord0 = stopToMapPos[getPrevItemArray(_infoLines.transportLines[x].stations, 1 * s + --enterOffset).stopId]; coord0Id = coord0[0] << 12 | coord0[1]; }
                if (coord1Id == coord2Id) continue;
                while (coord2Id == coord3Id) { coord3 = stopToMapPos[getNextItemArray(_infoLines.transportLines[x].stations, 1 * s + ++exitOffset).stopId]; coord3Id = coord3[0] << 12 | coord3[1]; }

                if (coord1Id > coord2Id) {
                    let tmp = coord1Id;
                    coord1Id = coord2Id;
                    coord2Id = tmp;
                    tmp = coord1;
                    coord1 = coord2;
                    coord2 = tmp;
                    tmp = coord3;
                    coord3 = coord0;
                    coord0 = tmp;
                }
                let segmentId = coord1Id.toString(16).padStart(8, "0") + coord2Id.toString(16).padStart(8, "0");
                var segmentAngle = Math.atan2(coord2[0] - coord1[0], coord2[1] - coord1[1]) * 180 / Math.PI;
                // let angle1 = segmentAngle//coord0 == coord2 ? -segmentAngle : Math.atan2(coord1[0] + coord0[0], coord1[1] - coord0[1]) * 180 / Math.PI;
                // let angle2 = segmentAngle//coord1 == coord3 ? -segmentAngle : Math.atan2(-coord3[0] + coord2[0], coord3[1] - coord2[1]) * 180 / Math.PI;
                let lineObj = {
                    lineId: _infoLines.transportLines[x].lineId,
                    lineColor: _infoLines.transportLines[x].lineColor,
                    //coord1Angle: Math.round(((angle1 + 360) % 360) / 45) % 8,
                    // coord2Angle: Math.round(((angle2 + 360) % 360) / 45) % 8,
                };
                if (segments[segmentId] === undefined) {
                    segments[segmentId] = {
                        segmentTransportType: _infoLines.transportLines[x].transportType,
                        passingLines: [lineObj],
                        segmentAngle: Math.round(((segmentAngle + 360) % 360) / 45) % 8,
                        segmentId: segmentId
                    }
                } else {
                    segments[segmentId].passingLines.push(lineObj);
                }
            }
        }


        const segmentDirectionDeltas = [
            [0, -1],
            [-1, -1],
            [-1, 0],
            [-1, 1],
            [0, 1],
            [1, 1],
            [1, 0],
            [1, -1],
        ]
        const segmentDirectionDeltasPerp = segmentDirectionDeltas.map(x => x[0] != 0 && x[1] != 0 ? x.map(y => y * 0.7071) : x);


        function fillLineTemplateSvg(lineObj, coordinates) {
            return `<polyline points="${coordinates}" class="path${lineObj.transportType} _lid_${lineObj.lineId}" style='stroke:${lineObj.lineColor};stroke-width: ${getBorderSize(lineObj.transportType)}px' stroke-linejoin="round" stroke-linecap="round" />`;
        }


        /*
          lineId: _infoLines.transportLines[x].lineId,
                    coord1Angle: Math.round(((angle1 + 180) % 180) / 45),
                    coord2Angle: Math.round(((angle2 + 180) % 180) / 45),
                    coord1Direction: Math.sign(angle1) || 1,
                    coord1Direction: Math.sign(angle2) || 1,
        */

        var sizeMultiplier = 50;

        function fillSegmentTemplateSvg(segmentObj) {
            let result = "";
            let p1 = parseInt(segmentObj.segmentId.substr(0, 8), 16)
            let p2 = parseInt(segmentObj.segmentId.substr(8, 8), 16)
            let startPoint = [p1 >> 12, p1 & ((1 << 12) - 1)];
            let endPoint = [p2 >> 12, p2 & ((1 << 12) - 1)];

            let startingDirDelta = segmentDirectionDeltas[(4 + segmentObj.segmentAngle) % 8];
            let startingPerpDelta = segmentDirectionDeltasPerp[(2 + segmentObj.segmentAngle) % 8];

            let endingDirDelta = segmentDirectionDeltas[segmentObj.segmentAngle];
            let endingPerpDelta = segmentDirectionDeltasPerp[(segmentObj.segmentAngle + 2) % 8];

            let width = getBorderSize(segmentObj.segmentTransportType);
            for (let l in segmentObj.passingLines.sort(x => x.lineId)) {
                let line = segmentObj.passingLines[l]
                //     if (line.lineId != 187) continue;

                let diagOffset;
                if (startingDirDelta[0] == 0) {
                    diagOffset = Math.abs(Math.abs(startPoint[0] - endPoint[0]) - Math.abs(startPoint[1] - endPoint[1])) / 2;
                } else if (startingDirDelta[1] == 0) {
                    diagOffset = Math.abs(Math.abs(startPoint[1] - endPoint[1]) - Math.abs(startPoint[0] - endPoint[0])) / 2;
                } else {
                    diagOffset = Math.min(Math.abs(startPoint[0] - endPoint[0]), Math.abs(startPoint[1] - endPoint[1])) / 2;
                }

                let offsetVal = (l);

                let coordinates = [startPoint.map((x, i) => x * sizeMultiplier + 120 + startingPerpDelta[i] * width * offsetVal)];
                coordinates.push(startPoint.map((x, i) => (x + startingDirDelta[i] * diagOffset) * sizeMultiplier + 120 + startingPerpDelta[i] * width * offsetVal))

                coordinates.push(endPoint.map((x, i) => (x + endingDirDelta[i] * diagOffset) * sizeMultiplier + 120 + endingPerpDelta[i] * width * offsetVal))
                coordinates.push(endPoint.map((x, i) => x * sizeMultiplier + 120 + endingPerpDelta[i] * width * offsetVal));


                result += `<polyline  title="segmentObj.segmentAngle = ${segmentObj.segmentAngle}; line.coord1Angle = ${line.coord1Angle}; line.coord2Angle = ${line.coord2Angle}" points="${coordinates.map(x => x.map(x => x).join(',')).join(',')}" class="path${segmentObj.segmentTransportType} _lid_${line.lineId}" style='stroke:${line.lineColor};stroke-width: ${width}px' stroke-linejoin="round" stroke-linecap="round" />`;
            }
            return result;
        }

        function addStation(stationData) {
            let pos = stopToMapPos[stationData.stopId].map(x => x * sizeMultiplier + 120).join(",");
            let result = `<circle style="stroke:white; stroke-width:1" fill="white" r="6" cy="0" cx="0" transform="translate(${pos})" class="${stationData.linesPassing.map(x => `_lid_${x}`).join(" ")}" />`;
            let currentCircleSize = 6;
            let orderedLines = stationData.linesPassing.sort(x => -getBorderSize(_infoLines.transportLines[x].transportType))
            for (let lineIdx in orderedLines) {
                let data = _infoLines.transportLines[orderedLines[lineIdx]];
                let borderSize = getBorderSize(data.transportType);
                result += `<circle style="stroke:${data.lineColor}; stroke-width:${borderSize}" fill="transparent" r="${currentCircleSize + borderSize / 2}" cy="0" cx="0" transform="translate(${pos})" class="stationCircle _lid_${data.lineId}" />`;
                currentCircleSize += borderSize;
            }
            $("#stationsContainer").append(addStationLabel(stationData, currentCircleSize + 3));
            return result;
        }

        function addStationLabel(stationData, offset) {
            let pos = stopToMapPos[stationData.stopId].map(x => x * sizeMultiplier + 120);
            let writeAngle = (stationData.writeAngle + 360) % 360;
            return `<div class='stationContainer ${stationData.linesPassing.map(x => `_lid_${x}`).join(" ")} _tt_${_infoLines.transportLines[stationData.linesPassing[0]].transportType}' style='top: ${pos[1]}px; left: ${pos[0]}px;'>
            <p style='transform: rotate(${writeAngle}deg) translate(${offset}px, ${offset}px) scale(${stationData.linesPassing.reduce((y, x) => Math.max(y, getTextScale(_infoLines.transportLines[x].transportType)), 0)});'>
               <x style='${writeAngle >= 90 && writeAngle < 270 ? "display: flex;font-weight: inherit;text-align: right;" : ""}transform: rotate(${writeAngle > 45 && writeAngle < 315 ? -writeAngle : 0}deg)${writeAngle >= 45 && writeAngle < 270 ? `  translate(${writeAngle == 45 ? offset / 2 : writeAngle <= 135 ? offset : 0}px, ${writeAngle >= 135 ? offset : writeAngle == 45 ? -offset : 0}px);` : ""}'>${stationData.name}</x> 
            </p>
        </div>`
        }//${stationData.writeAngle >= 135 && stationData.writeAngle < 270 ? `<y>${stationData.name}</y>` : `<x>${stationData.name}</x>`}



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

        function getBorderSize(transportType) {
            switch (transportType) {
                case "Tram":
                    return 2;
                case "Train":
                    return 4;
                case "Metro":
                    return 4;
                case "Monorail":
                    return 4;
                case "Ship":
                    return 8;
                case "Airplane":
                    return 8;
                case "Bus":
                    return 2;
            }
        }
        function getTextScale(transportType) {
            switch (transportType) {
                case "Tram":
                    return .75;
                case "Train":
                    return 1.7;
                case "Metro":
                    return 1.7;
                case "Monorail":
                    return 1.7;
                case "Ship":
                    return 2;
                case "Airplane":
                    return 2;
                case "Bus":
                    return .666;
            }
        }

        function getLineBlockString(lineObj) {
            return `<div class="lineBlock _lid_${lineObj.lineId}" style="--lineColor: ${lineObj.lineColor}">` +
                `<div class="lineNumber lineNumber"><svg width="45" height="45" xmlns="http://www.w3.org/2000/svg"><path d="${getPathForTransportVehicle(lineObj.transportType, lineObj.vehicleType)}" style="fill: var(--lineColor)"/><text lengthAdjust="${lineObj.lineStringIdentifier.length > 2 ? "spacingAndGlyphs" : "spacing"}" x="22.5" y="22.5" textLength="42">${lineObj.lineStringIdentifier}</text></svg></div>` +
                `</div>`;
        }

        /*activeDay: true
         ​
        activeNight: true
         ​
        lineColor: "#6FE9B3"
         ​
        lineId: 40
         ​
        lineName: "[3104] ㎡ Britania - ㎡ Di Saboia"
         ​
        lineNumber: 3104
         ​
        lineStringIdentifier: "3104"
         ​
        simmetryRange: null
         ​
        stations: Array(13) [ {…}, {…}, {…}, … ]
         ​
        subservice: "PublicTransportTram"
         ​
        transportType: "Tram"
         ​
        vehicleType: "Tram"*/

        $(document).ready(function () {
            $("#linesPanel #content").html("");
            let lines = Object.values(_infoLines.transportLines);
            lines.sort(function (a, b) {
                if (a.transportType === b.transportType) {
                    if (a.vehicleType === b.vehicleType) {
                        return a.lineStringIdentifier.localeCompare(b.lineStringIdentifier)
                    }
                    return a.vehicleType.localeCompare(b.vehicleType)
                }
                return a.transportType.localeCompare(b.transportType)
            });
            var ordSegments = Object.keys(segments).map(x => { return { "k": x, "v": segments[x] } }).sort(x => -getBorderSize(x.v.segmentTransportType))
            for (let x in ordSegments) {
                $("#map")[0].innerHTML += fillSegmentTemplateSvg(ordSegments[x].v);
            }
            for (let x in lines) {
                $("#linesPanel #content").append(getLineBlockString(lines[x]))
                //  if (lines[x].transportType == "Train")
                let lineId = lines[x].lineId;
                $("#map")[0].innerHTML += lines[x].stations.map(y => y.linesPassing[0] != lineId ? "" : addStation(y)).join("\n");
            }
        });