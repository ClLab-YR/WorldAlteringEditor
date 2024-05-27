﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TSMapEditor.GameMath;
using TSMapEditor.Misc;
using TSMapEditor.Models;
using TSMapEditor.Rendering;
using TSMapEditor.UI;

namespace TSMapEditor.Mutations.Classes
{
    /// <summary>
    /// A mutation for drawing cliffs.
    /// </summary>
    public class DrawCliffMutation : Mutation
    {
        public DrawCliffMutation(IMutationTarget mutationTarget, List<Point2D> cliffPath, CliffType cliffType, CliffSide startingSide, int randomSeed, byte extraHeight) : base(mutationTarget)
        {
            if (cliffPath.Count < 2)
            {
                throw new ArgumentException(nameof(DrawCliffMutation) +
                    ": to draw a connected tile at least 2 path vertices are required.");
            }

            this.cliffPath = cliffPath;
            this.cliffType = cliffType;
            this.startingSide = startingSide;

            this.originLevel = mutationTarget.Map.GetTile(cliffPath[0]).Level + extraHeight;
            this.random = new Random(randomSeed);
        }

        private struct CliffUndoData
        {
            public Point2D CellCoords;
            public int TileIndex;
            public byte SubTileIndex;
            public byte Level;
        }

        private readonly List<CliffUndoData> undoData = new List<CliffUndoData>();

        private readonly List<Point2D> cliffPath;
        private readonly CliffType cliffType;
        private readonly CliffSide startingSide;
        
        private readonly int originLevel;
        private readonly Random random;

        private CliffAStarNode lastNode;
        private const int MaxIterations = 50;

        public override void Perform()
        {
            lastNode = null;

            for (int i = 0; i < cliffPath.Count - 1; i++)
            {
                FindCliffPath(cliffPath[i], cliffPath[i + 1]);
            }

            PlaceCliffs(lastNode);

            MutationTarget.InvalidateMap();
        }

        private void FindCliffPath(Point2D start, Point2D end)
        {
            PriorityQueue<CliffAStarNode, float> openSet = new PriorityQueue<CliffAStarNode, float>();

            CliffAStarNode bestNode = null;
            float bestDistance = float.PositiveInfinity;

            if (lastNode == null)
            {
                lastNode = CliffAStarNode.MakeStartNode(start, end, startingSide);
            }
            else
            {
                // Go back one step if we can, since we didn't know we needed to turn yet
                // and it's likely not gonna be very nice
                lastNode = lastNode.Parent ?? lastNode;
                lastNode.Destination = end;
            }

            int iterations = 0;
            openSet.Enqueue(lastNode, lastNode.FScore);

            while (openSet.Count > 0)
            {
                CliffAStarNode currentNode = openSet.Dequeue();
                openSet.EnqueueRange(currentNode.GetNextNodes(cliffType.Tiles).Select(node => (node, node.FScore)));
                
                if (currentNode.HScore < bestDistance)
                {
                    bestNode = currentNode;
                    bestDistance = currentNode.HScore;
                    iterations = 0;
                }
                else
                {
                    iterations++;
                }


                if (bestDistance == 0 || iterations > MaxIterations)
                    break;
            }

            lastNode = bestNode;
        }

        private void PlaceCliffs(CliffAStarNode endNode)
        {
            var node = endNode;
            while (node != null)
            {
                if (node.Tile != null)
                {
                    var tileSet = MutationTarget.Map.TheaterInstance.Theater.FindTileSet(node.Tile.TileSetName);
                    if (tileSet != null)
                    {
                        var tileImage = MutationTarget.TheaterGraphics.GetTileGraphics(tileSet.StartTileIndex + node.Tile.IndicesInTileSet.GetRandomElement(random));
                        PlaceTile(tileImage, new Point2D((int)node.Location.X, (int)node.Location.Y));
                    }
                    else
                    {
                        throw new INIConfigException($"Tile Set {node.Tile.TileSetName} not found when placing cliffs!");
                    }
                }

                node = node.Parent;
            }
        }

        private void PlaceTile(TileImage tile, Point2D targetCellCoords)
        {
            if (tile == null)
                return;

            for (int i = 0; i < tile.TMPImages.Length; i++)
            {
                MGTMPImage image = tile.TMPImages[i];
                if (image.TmpImage == null)
                    continue;

                int cx = targetCellCoords.X + i % tile.Width;
                int cy = targetCellCoords.Y + i / tile.Width;

                var mapTile = MutationTarget.Map.GetTile(cx, cy);
                if (mapTile != null)
                {
                    undoData.Add(new CliffUndoData()
                    {
                        CellCoords = new Point2D(cx, cy),
                        TileIndex = mapTile.TileIndex,
                        SubTileIndex = mapTile.SubTileIndex,
                        Level = mapTile.Level
                    });

                    mapTile.ChangeTileIndex(tile.TileID, (byte)i);
                    mapTile.Level = (byte)Math.Min(originLevel + image.TmpImage.Height, Constants.MaxMapHeightLevel);
                }
            }
        }

        public override void Undo()
        {
            for (int i = undoData.Count - 1; i >= 0; i--)
            {
                var data = undoData[i];
                var mapTile = MutationTarget.Map.GetTile(data.CellCoords);

                if (mapTile != null)
                {
                    mapTile.ChangeTileIndex(data.TileIndex, data.SubTileIndex);
                    mapTile.Level = data.Level;
                }
            }

            undoData.Clear();
            MutationTarget.InvalidateMap();
        }
    }
}