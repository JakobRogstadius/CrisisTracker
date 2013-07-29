<?xml version="1.0" encoding="UTF-8"?>
<!-- 
 /**
 * ====================================================================
 * About
 * ====================================================================
 * All XSLT Minesweeper
 * @version @sarissa.version@
 * @author: Copyright Sean Whalen
 *
 *	This is the template that determines which squares to reveal after a square has been clicked.
 * It expects to receive an XML document that contains a list of XY points with "isBomb" and "nbc" attributes, 
 * and an element containing the square that was clicked.  It returns an xml document of all the squares to
 * reveal, with their colors and neighboring bomb counts ("nbc"), based on the XML document it received.  
 *
 * 	The task is to return all the squares that surround the square that was clicked 
 * that have an "isBomb" value of zero and an "nbc" value of zero.  This is in some sense a fill-algorithm, 
 * similar to filling an area of bitmap with a new color, because the code needs to find a complete 
 * set of points within some boundry.  In this case the boundry is defined as square elements 
 * with non-zero "nbc" values.  
 *
 *	The algorithm works by defining an area which has been already tested for boundries, and then 
 * testing the squares that are plus-or-minus-one away from the perimiter of that area.  Initially,
 * the area is seeded with the square that was clicked.  
 *
 * 	The template parameter $alreadyRevealed accumulates the squares that have been tested 
 * and should be revealed.  It is seeded with the square that was clicked.  
 *
 * 	The variable $field contains all the squares that are not in the $alreadyRevealed set.  It is the set of
 * squares that will be tested during the current iteration.  Note that the $field excludes bombs, 
 * because they are never revealed, but it includes the squares with non-zero "nbc" values.  This is because
 * those non-zero squares are revealed to the user at the end of the transformation, even though they do not
 * contribute to the search in the next iteration of the template.  
 *
 *	The parameter $recentSet contains the set of squares revealed by the most recent iteration 
 * of the template.  This is a much smaller set than $alreadyRevealed; $recentSet represents a perimeter, 
 * while $alreadyRevealed represents an area, which is much larger.  Originally I had used the $alreadyRevealed 
 * set to compare against the $field, and that was much slower.  
 *
 * 	Only the members of the $recentSet that have "nbc" values of zero are used in the search.  They are 
 * copied into a variable called $zeros.  The squares with non-zero nbc values represent the border, and 
 * they stop the search.
 *
 *	The variable $revealing is the intersection of the current set of $zeros with the remaining members of 
 * the $field.  I'm using the term intersection a little loosely here, because the XY values of the $zeros set 
 * need to be adjusted by +1 and -1 in order to find squares that are just beyond the current perimeter
 * of the $zeros set.  
 *
 *	The simplest way to describe the search is to say that it is looking for any squares 
 * WHERE (ABS(field.H - zeros.H ) =1 OR ABS($field.V - $field.V ) =1).  But I couldn't come up with XSLT syntax
 * that was that concise.  
 *	The algorithm is done when the number of new squares that are revealed by an iteration is zero.  
 * Then the code outputs the accumulated revealed squares with their color values.
 *
 *
 *
 * ====================================================================
 * Licence
 * ====================================================================
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License version 2 or
 * the GNU Lesser General Public License version 2.1 as published by
 * the Free Software Foundation (your choice of the two).
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License or GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * or GNU Lesser General Public License along with this program; if not,
 * write to the Free Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 * or visit http://www.gnu.org
 *
 */
 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output method="html"/>
	<xsl:template match="/">
		<xsl:call-template name="initReveal"/>
	</xsl:template>
	<xsl:template name="initReveal">
		<xsl:variable name="hClick" select="SweeperMap/click[last()]/@h"/>
		<xsl:variable name="vClick" select="SweeperMap/click[last()]/@v"/>
		<xsl:variable name="clicked" select="SweeperMap/square[@h=$hClick and @v=$vClick]"/>
		<xsl:call-template name="neighborsXY">
			<xsl:with-param name="alreadyRevealed" select="$clicked"/>
			<xsl:with-param name="recentSet" select="$clicked"/>
		</xsl:call-template>
	</xsl:template>
	<xsl:template name="neighborsXY">
		<xsl:param name="alreadyRevealed"/>
		<xsl:param name="recentSet"/>
		<xsl:variable name="zeros" select=" $recentSet[@nbc = 0 ]"/>
		<xsl:variable name="field" select="SweeperMap/square[(@isBomb != -1 and @isRevealed = 0 and
			not (@sqID = $alreadyRevealed/@sqID )) ]"/>
		<xsl:variable name="revealing" select="$field[              (       (concat(@h  -1 ,'/', @v
			) = $zeros/@sqID)             or (concat(@h  +1 ,'/', @v   ) = $zeros/@sqID)  or
			(concat(@h  -1 ,'/', @v -1) = $zeros/@sqID)  or (concat(@h  -1 ,'/', @v +1) =
			$zeros/@sqID)  or (concat(@h     ,'/', @v   ) = $zeros/@sqID)   or (concat(@h  +1 ,'/',
			@v +1) = $zeros/@sqID)  or (concat(@h  +1 ,'/', @v -1) = $zeros/@sqID)  or (concat(@h
			,'/', @v +1) = $zeros/@sqID)  or (concat(@h     ,'/', @v -1) = $zeros/@sqID)    )] "/>
		<xsl:variable name="totRevealed" select="$revealing | $alreadyRevealed"/>
		<xsl:choose>
			<xsl:when test="count($revealing[@nbc = 0 ] ) &gt; 0">
				<xsl:call-template name="neighborsXY">
					<xsl:with-param name="alreadyRevealed" select="$totRevealed "/>
					<xsl:with-param name="recentSet" select="$revealing"/>
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<xsl:element name="SweeperMap">
					<xsl:for-each select="$totRevealed">
						<xsl:variable name="nbcColor">
							<xsl:choose>
								<xsl:when test="./@nbc = '0' ">#AA3344</xsl:when>
								<xsl:when test="./@nbc = '1' ">#0066FF</xsl:when>
								<xsl:when test="./@nbc = '2' ">#009900</xsl:when>
								<xsl:when test="./@nbc = '3' ">#FF0000</xsl:when>
								<xsl:when test="./@nbc = '4' ">#663399</xsl:when>
								<xsl:when test="./@nbc = '5' ">#ff8800</xsl:when>
								<xsl:when test="./@nbc = '6' ">#0088AA</xsl:when>
							</xsl:choose>
						</xsl:variable>
						<xsl:element name="square">
							<xsl:attribute name="h">
								<xsl:value-of select="./@h"/>
							</xsl:attribute>
							<xsl:attribute name="v">
								<xsl:value-of select="./@v"/>
							</xsl:attribute>
							<xsl:attribute name="isRevealed">1</xsl:attribute>
							<xsl:attribute name="nbc">
								<xsl:value-of select="./@nbc"/>
							</xsl:attribute>
							<xsl:attribute name="nbcColor">
								<xsl:value-of select="$nbcColor"/>
							</xsl:attribute>
						</xsl:element>
					</xsl:for-each>
				</xsl:element>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
</xsl:stylesheet>
