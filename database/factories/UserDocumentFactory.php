<?php

namespace Database\Factories;

use App\Models\UserDocument;
use Illuminate\Database\Eloquent\Factories\Factory;

class UserDocumentFactory extends Factory
{
    /**
     * The name of the factory's corresponding model.
     *
     * @var string
     */
    protected $model = UserDocument::class;

    /**
     * Define the model's default state.
     *
     * @return array
     */
    public function definition()
    {
        return [
            'user_id'=>User::factory(),
            'file_id'=>File::factory(),
            'category_id'=>Category::factory(),
            'isPublic'=>$this->faker->boolval()
        ];
    }
}
