<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

class CreateMessagesTable extends Migration
{
    /**
     * Run the migrations.
     *
     * @return void
     */
    public function up()
    {
        Schema::create('messages', function (Blueprint $table) {
            $table->id();
            $table->unsignedBigInteger('from_id');
            $table->unsignedBigInteger('to_id');
            $table->string('content');
            $table->boolean('fromHidden')->default(false);//per si no ho vol veure
            $table->boolean('toHidden')->default(false);
            $table->boolean('toCheck');//per si qui ho ha rebut vol denunciar-ho
            $table->softDeletes();//aixi es dessen fins que s'esborrin de forma controlada
            $table->timestamps();
        });
    }

    /**
     * Reverse the migrations.
     *
     * @return void
     */
    public function down()
    {
        Schema::dropIfExists('messages');
    }
}
