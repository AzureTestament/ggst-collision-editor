# ggst-collision-editor
A GUI hitbox/hurtbox editor for Guilty Gear -Strive-. Based off of Labreezy's old bb-collision-editor. 

Very bare-bones at the moment. Current capabilities are limited to editing existing boxes, with no way of adding or removing boxes or sprites. Additionally, changing sprites will lose all progress. You must save the entire PAC in order to keep changes from the current sprite.

To get the necessary PAC file, use https://github.com/super-continent/ggst-bbs-unpacker on RED/Content/Chara/XXX/Common/Data/COL_XXX.uexp, where XXX is the character ID. Be sure to extract with the .PAC extension, otherwise the collision editor will ignore the file in the open file dialog.

Note that this editor is incompatible with other ArcSys game hitboxes. For BlazBlue, Guilty Gear Xrd, and Dragon Ball FighterZ, use Labreezy's editor. There is currently no editor for Granblue Fantasy Versus.
