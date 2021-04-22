
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
                if (dist < minDistance && dist > 0) {
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

        var stationIdExitAngleCount = {};
        var stationIdExitAngleDrawCount = {};

        function recalculate() {
            stopToTgPos = {};
            tgPosToStop = {};
            tgPosToRealX = {};
            tgPosToRealY = {};
            stopToMapPos = {};
            stationIdExitAngleCount = {};

            for (let x in _infoLines.transportLines) {
                //   if (_infoLines.transportLines[x].transportType != "Train") continue;
                for (let s in _infoLines.transportLines[x].stations) {
                    let stopId = _infoLines.transportLines[x].stations[s].stopId;
                    SafeStore2(stopId, stopsPositionList[stopId][0], stopsPositionList[stopId][1]);
                }
            }

            let xSortedKeys = Object.keys(tgPosToRealX).map(x => x * 1).sort((x, y) => x - y);
            let ySortedKeys = Object.keys(tgPosToRealY).map(x => x * 1).sort((x, y) => y - x);

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
                let stationStartId = _infoLines.transportLines[x].stations[s].id;
                let stationEndId = getNextItemArray(_infoLines.transportLines[x].stations, 1 * s).id;
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
                    tmp = stationStartId;
                    stationStartId = stationEndId;
                    stationEndId = tmp;
                }
                let segmentId = coord1Id.toString(16).padStart(8, "0") + coord2Id.toString(16).padStart(8, "0");
                var segmentAngle = Math.atan2(coord2[0] - coord1[0], coord2[1] - coord1[1]) * 180 / Math.PI;
                let segmentAngleIdx = Math.round(((segmentAngle + 360) % 360) / 45) % 8;
                // let angle1 = segmentAngle//coord0 == coord2 ? -segmentAngle : Math.atan2(coord1[0] + coord0[0], coord1[1] - coord0[1]) * 180 / Math.PI;
                // let angle2 = segmentAngle//coord1 == coord3 ? -segmentAngle : Math.atan2(-coord3[0] + coord2[0], coord3[1] - coord2[1]) * 180 / Math.PI;
                let lineObj = {
                    lineId: _infoLines.transportLines[x].lineId,
                    lineColor: _infoLines.transportLines[x].lineColor,
                    lineTransportType: _infoLines.transportLines[x].transportType,
                };
                if (segments[segmentId] === undefined) {
                    segments[segmentId] = {
                        passingLines: [lineObj],
                        segmentAngle: segmentAngleIdx,
                        segmentId: segmentId,
                        stationStartId: stationStartId,
                        stationEndId: stationEndId
                    }
                } else {
                    segments[segmentId].passingLines.push(lineObj);
                }
                var idxStationStart = (stationStartId << 3) + (segmentAngleIdx + 4) % 8;
                var idxStationEnd = (stationEndId << 3) + segmentAngleIdx % 8;
                if (stationIdExitAngleCount[idxStationStart] === undefined) stationIdExitAngleCount[idxStationStart.toString()] = 0;
                if (stationIdExitAngleCount[idxStationEnd] === undefined) stationIdExitAngleCount[idxStationEnd.toString()] = 0;
                let width = getBorderSize(_infoLines.transportLines[x].transportType);
                stationIdExitAngleCount[idxStationStart] += width;
                stationIdExitAngleCount[idxStationEnd] += width;
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
        const transformOrigins = [
            ["center", "bottom"],
            ["right", "bottom"],
            ["right", "center"],
            ["right", "top"],
            ["center", "top"],
            ["left", "top"],
            ["left", "center"],
            ["left", "bottom"],
        ]
        const segmentDirectionDeltasPerp = segmentDirectionDeltas.map(x => x[0] != 0 && x[1] != 0 ? x.map(y => y * 0.7071) : x);

        /*
           lineId: _infoLines.transportLines[x].lineId,
                     coord1Angle: Math.round(((angle1 + 180) % 180) / 45),
                     coord2Angle: Math.round(((angle2 + 180) % 180) / 45),
                     coord1Direction: Math.sign(angle1) || 1,
                     coord1Direction: Math.sign(angle2) || 1,
         */

        var sizeMultiplier = 100;

        function fillSegmentTemplateSvg(segmentObj, line) {
            let result = "";
            let p1 = parseInt(segmentObj.segmentId.substr(0, 8), 16)
            let p2 = parseInt(segmentObj.segmentId.substr(8, 8), 16)
            let startPoint = [p1 >> 12, p1 & ((1 << 12) - 1)];
            let endPoint = [p2 >> 12, p2 & ((1 << 12) - 1)];

            let angleStart = (4 + segmentObj.segmentAngle) % 8;
            let angleEnd = segmentObj.segmentAngle;

            let startingDirDelta = segmentDirectionDeltas[(4 + segmentObj.segmentAngle) % 8];
            let startingPerpDelta = segmentDirectionDeltasPerp[(2 + segmentObj.segmentAngle) % 8];

            let endingDirDelta = segmentDirectionDeltas[segmentObj.segmentAngle];
            let endingPerpDelta = segmentDirectionDeltasPerp[(segmentObj.segmentAngle + 2) % 8];

            let baseStartStationId = (segmentObj.stationStartId << 3) + angleStart;
            let baseEndStationId = (segmentObj.stationEndId << 3) + angleEnd;

            //     if (line.lineId != 187) continue;

            let width = getBorderSize(line.lineTransportType);
            let diagOffset;
            stationIdExitAngleDrawCount[baseStartStationId] ??= 0;
            stationIdExitAngleDrawCount[baseEndStationId] ??= 0;
            let offsetValStart = (stationIdExitAngleDrawCount[baseStartStationId]) - stationIdExitAngleCount[baseStartStationId] / 2;
            let offsetValEnd = (stationIdExitAngleDrawCount[baseEndStationId]) - stationIdExitAngleCount[baseEndStationId] / 2;
            let deltaOffset = Math.abs(offsetValStart - offsetValEnd);
            if (deltaOffset > 0 && deltaOffset < width) {
                if (offsetValStart > offsetValEnd) {
                    stationIdExitAngleDrawCount[baseEndStationId] += deltaOffset;
                    offsetValEnd = offsetValStart;
                } else {
                    stationIdExitAngleDrawCount[baseStartStationId] += deltaOffset;
                    offsetValStart = offsetValEnd;
                }
            }

            stationIdExitAngleDrawCount[baseStartStationId] += width;
            stationIdExitAngleDrawCount[baseEndStationId] += width;

            let lineStartPoint = startPoint.map((x, i) => x + (startingPerpDelta[i] * offsetValStart + startingPerpDelta[i] * width / 2) / sizeMultiplier)
            let lineEndPoint = endPoint.map((x, i) => x + (endingPerpDelta[i] * offsetValEnd + startingPerpDelta[i] * width / 2) / sizeMultiplier)

            let spacingOffset = 0;

            if (startingDirDelta[0] == 0) {
                diagOffset = Math.abs(Math.abs(lineStartPoint[0] - lineEndPoint[0]) - Math.abs(lineStartPoint[1] - lineEndPoint[1])) / 2;
                spacingOffset = Math.sign(offsetValStart) / .707;
            } else if (startingDirDelta[1] == 0) {
                diagOffset = Math.abs(Math.abs(lineStartPoint[1] - lineEndPoint[1]) - Math.abs(lineStartPoint[0] - lineEndPoint[0])) / 2;
                spacingOffset = Math.sign(offsetValStart) / .707;

            } else {
                diagOffset = Math.min(Math.abs(lineStartPoint[0] - lineEndPoint[0]), Math.abs(lineStartPoint[1] - lineEndPoint[1])) / 2;
                spacingOffset = Math.sign(offsetValStart) * Math.sign(startingDirDelta[1]) * Math.sign(startingDirDelta[0]);
            }

            switch (angleStart) {
                case 0:
                case 2:
                case 4:
                case 6:
                    spacingOffset = Math.sign(offsetValStart) / .707;
                    break;
                case 1: spacingOffset = Math.sign(offsetValStart) * Math.sign(startingDirDelta[1]) * Math.sign(startingDirDelta[0]);
                    break;
                case 3: spacingOffset = Math.sign(offsetValStart) * Math.sign(startingDirDelta[1]) * Math.sign(startingDirDelta[0]);
                    break;
                case 5: spacingOffset = -Math.sign(offsetValStart) * Math.sign(startingDirDelta[1]) * Math.sign(startingDirDelta[0]);
                    break;
                case 7: spacingOffset = -Math.sign(offsetValStart) * Math.sign(startingDirDelta[1]) * Math.sign(startingDirDelta[0]);
                    break;
            }

            let coordinates = [lineStartPoint.map((x, i) => x * sizeMultiplier + 120)];
            coordinates.push(lineStartPoint.map((x, i) => (x + startingDirDelta[i] * diagOffset) * sizeMultiplier + 120 - startingDirDelta[i] * ((offsetValStart - offsetValEnd) / 2 * spacingOffset - (offsetValStart - offsetValEnd) / 2)))

            coordinates.push(lineEndPoint.map((x, i) => (x + endingDirDelta[i] * diagOffset) * sizeMultiplier + 120 - startingDirDelta[i] * ((offsetValStart - offsetValEnd) / 2 * spacingOffset - (offsetValStart - offsetValEnd) / 2)))
            coordinates.push(lineEndPoint.map((x, i) => x * sizeMultiplier + 120));


            result += `<polyline points="${coordinates.map(x => x.map(x => x).join(',')).join(',')}" class="path${line.lineTransportType}" style='stroke:antiquewhite;stroke-width: ${width}px' stroke-linejoin="round" stroke-linecap="round" />`;
            result += `<polyline points="${coordinates.map(x => x.map(x => x).join(',')).join(',')}" class="path${line.lineTransportType} _lid_${line.lineId}" style='stroke:${line.lineColor};stroke-width: ${width * .8}px' stroke-linejoin="round" stroke-linecap="round" />`;

            return result;
        }

        function addStation(stationData) {
            if ($(`#stationPoint${stationData.id}`)[0]) return;
            let pos = stopToMapPos[stationData.stopId].map(x => x * sizeMultiplier + 120).join(",");
            let result = `<circle id="stationPoint${stationData.id}" style="stroke:antiquewhite; stroke-width:1" fill="white" r="6" cy="0" cx="0" transform="translate(${pos})" class="${stationData.linesPassing.map(x => `_lid_${x}`).join(" ")}" />`;
            let currentCircleSize = 6;
            let orderedLines = stationData.linesPassing.sort((a, b) => getBorderSize(_infoLines.transportLines[b].transportType) - getBorderSize(_infoLines.transportLines[a].transportType))
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
            if ($(`#station${stationData.id}`)[0]) return;
            let pos = stopToMapPos[stationData.stopId].map(x => x * sizeMultiplier + 120);
            let writeAngle = 0;
            for (let x = 6; x < 14; x++) {
                if (!stationIdExitAngleCount[(stationData.id << 3) + (x % 8)]) {
                    writeAngle = x % 8;
                    break;
                }
            }
            let dir = segmentDirectionDeltasPerp[writeAngle];
            let targetScale = stationData.linesPassing.reduce((y, x) => Math.max(y, getTextScale(_infoLines.transportLines[x].transportType)), 0);
            let xElemOffset = 0;
            switch (writeAngle) {
                case 0:
                    xElemOffset = -5; break;
                case 1:
                    xElemOffset = -90; break;
                case 2:
                    xElemOffset = -100; break;
                case 3:
                    xElemOffset = -110; break;
                case 4:
                    xElemOffset = 12.5 / Math.sqrt(targetScale); break;
                case 5:
                    xElemOffset = 12.5 / Math.sqrt(targetScale); break;
                case 7:
                    xElemOffset = -10; break;

            }
            return `<div id="station${stationData.id}" class='stationContainer ${stationData.linesPassing.map(x => `_lid_${x}`).join(" ")} _tt_${_infoLines.transportLines[stationData.linesPassing[0]].transportType}' style='top: ${pos[1]}px; left: ${pos[0]}px;'>
            <p style='transform-origin: center;transform:  translate(${(offset) * (dir[0])}px,${(offset) * (dir[1])}px) scale(${targetScale}) '>
               <x title='writeAngle = ${writeAngle}; stationId = ${stationData.id}' style='${(writeAngle < 4 && writeAngle != 0) ? "text-align: right;" : ""
                } transform: rotate(${writeAngle % 4 == 1 || writeAngle == 4 ? 45 : writeAngle % 4 == 3 || writeAngle == 0 ? -45 : 0}deg) translate(${xElemOffset}%,0); width: ${60 / targetScale}px; ${writeAngle == 6 ? "margin:-33% 33% ;" : ""}'>${stationData.name}</x>
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
                    return 4;
                case "Train":
                    return 8;
                case "Metro":
                    return 8;
                case "Monorail":
                    return 8;
                case "Ship":
                    return 16;
                case "Airplane":
                case "Helicopter":
                    return 16;
                case "Bus":
                case "Trolleybus":
                    return 4;
            }
        }
        function getTextScale(transportType) {
            switch (transportType) {
                case "Tram":
                    return .75;
                case "Train":
                    return 1.5;
                case "Metro":
                    return 1.5;
                case "Monorail":
                    return 1.5;
                case "Ship":
                    return 2;
                case "Airplane":
                case "Helicopter":
                    return 2;
                case "Bus":
                case "Trolleybus":
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
            var ordSegments = Object.keys(segments).map(x => {
                return { "k": x, "v": segments[x] }
            }).map(x => x.v.passingLines.map(y => {
                return { k: x.k, segment: x.v, line: y, size: getBorderSize(y.lineTransportType) }
            })
            ).reduce((p, x) => p.concat(x), [])
            ordSegments.sort((a, b) => b.size - a.size || b.line.id - a.line.id)
            var stationIdExitAngleDrawCount = {};
            for (let x in ordSegments) {
                $("#map")[0].innerHTML += fillSegmentTemplateSvg(ordSegments[x].segment, ordSegments[x].line,);
            }
            for (let x in lines) {
                $("#linesPanel #content").append(getLineBlockString(lines[x]))
                //  if (lines[x].transportType == "Train")
                let lineId = lines[x].lineId;
                $("#map")[0].innerHTML += lines[x].stations.map(y => y.linesPassing[0] != lineId ? "" : addStation(y)).join("\n");
            }
        });